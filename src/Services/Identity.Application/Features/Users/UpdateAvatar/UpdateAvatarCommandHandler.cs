using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Providers;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Dtos.Storage;
using Identity.Domain.Entities;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Identity.Application.Features.Users.UpdateAvatar;

public sealed class UpdateAvatarCommandHandler : ICommandHandler<UpdateAvatarCommand, Result<UpdateAvatarResponse>>
{
  private readonly ILogger<UpdateAvatarCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly ResiliencePipeline _databasePipeline;

  private readonly ResiliencePipeline _storagePipeline;

  private readonly IStorageProvider _storageProvider;

  private readonly IUnitOfWork _unitOfWork;

  public UpdateAvatarCommandHandler(
    IUserRepository repository,
    IStorageProvider storageProvider,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<UpdateAvatarCommandHandler> logger,
    IUnitOfWork unitOfWork)
  {
    _repository = repository;
    _storageProvider = storageProvider;
    _logger = logger;
    _databasePipeline = pipelineProvider.GetPipeline("database-operations");
    _storagePipeline = pipelineProvider.GetPipeline("storage-operations");
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<UpdateAvatarResponse>> Handle(
    UpdateAvatarCommand request,
    CancellationToken cancellationToken)
  {
    User? user = await _databasePipeline.ExecuteAsync(async ct => await _repository.FindByIdAsync(
        request.UserId,
        ct
      )
    );

    if (user is null)
    {
      _logger.LogWarning(
        "Avatar update attempted for non-existent user {UserId}",
        request.UserId);

      return Result<UpdateAvatarResponse>.Failure(Error.NotFound("User not registered"));
    }

    string? previewImageName = user.UserAvatar?.AvatarImageName;

    StorageUploadResponseDto uploadedImage = await _storagePipeline.ExecuteAsync(async ct =>
      await _storageProvider.UploadImage(
        request.File,
        ct
      )
    );

    _logger.LogInformation(
      "New avatar uploaded to storage for user {UserId}. BlobName: {BlobName}",
      user.Id, uploadedImage.Name);

    try
    {
      bool isNewAvatar = user.UserAvatar is null;

      user.SetAvatar(uploadedImage.Uri, uploadedImage.Name, uploadedImage.ContentType);

      await _databasePipeline.ExecuteAsync(async ct =>
      {
        _repository.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
      });

      if (!isNewAvatar && previewImageName is not null)
      {
        await DeletePreviousImageSafelyAsync(previewImageName, user.Id);
      }

      string action = isNewAvatar ? "created" : "updated";

      _logger.LogInformation(
        "Avatar {Action} successfully for user {UserId}. BlobName: {BlobName}",
        action, user.Id, uploadedImage.Name);

      return Result<UpdateAvatarResponse>.Success(new UpdateAvatarResponse(action));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex,
        "Failed to update avatar in database for user {UserId}. " +
        "Rolling back storage upload. BlobName: {BlobName}",
        user.Id, uploadedImage.Name);

      await RollbackUploadSafelyAsync(uploadedImage.Name, user.Id);

      return Result<UpdateAvatarResponse>.Failure(
        Error.Failure("Failed to update avatar. Please try again."));
    }
  }

  private async Task DeletePreviousImageSafelyAsync(
    string imageName,
    Guid userId
  )
  {
    try
    {
      await _storagePipeline.ExecuteAsync(async cancel => await _storageProvider.DeleteImage(
          imageName,
          cancel
        )
      );

      _logger.LogInformation(
        "Previous avatar deleted from storage for user {UserId}. BlobName: {BlobName}",
        userId, imageName);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(
        ex,
        "Failed to delete previous avatar from storage for user {UserId}. " +
        "BlobName: {BlobName}. File may require manual cleanup.",
        userId, imageName);
    }
  }

  private async Task RollbackUploadSafelyAsync(
    string imageName,
    Guid userId
  )
  {
    try
    {
      await _storagePipeline.ExecuteAsync(async ct => await _storageProvider.DeleteImage(
          imageName,
          ct
        )
      );

      _logger.LogInformation(
        "Storage rollback successful for user {UserId}. BlobName: {BlobName}",
        userId, imageName);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex,
        "Storage rollback FAILED for user {UserId}. " +
        "Orphaned blob requires manual cleanup. BlobName: {BlobName}",
        userId, imageName);
    }
  }
}
