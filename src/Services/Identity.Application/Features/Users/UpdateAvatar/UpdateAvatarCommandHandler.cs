using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Providers;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Dtos.Storage;
using Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.UpdateAvatar;

public sealed class UpdateAvatarCommandHandler : ICommandHandler<UpdateAvatarCommand, Result<UpdateAvatarResponse>>
{
  private readonly IUserRepository _repository;

  private readonly ILogger<UpdateAvatarCommandHandler> _logger;

  private readonly IStorageProvider _storageProvider;

  private readonly IUnitOfWork _unitOfWork;

  public UpdateAvatarCommandHandler(
    IUserRepository repository,
    IStorageProvider storageProvider,
    ILogger<UpdateAvatarCommandHandler> logger,
    IUnitOfWork unitOfWork)
  {
    _repository = repository;
    _storageProvider = storageProvider;
    _logger = logger;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<UpdateAvatarResponse>> Handle(
    UpdateAvatarCommand request,
    CancellationToken cancellationToken)
  {
    User? user = await _repository.FindByIdAsync(request.UserId, cancellationToken);

    if (user is null)
    {
      _logger.LogWarning(
        "Avatar update attempted for non-existent user {UserId}",
        request.UserId);

      return Result<UpdateAvatarResponse>.Failure(Error.NotFound("User not registered"));
    }

    string? previewImageName = user.UserAvatar?.AvatarImageName;

    StorageUploadResponseDto uploadedImage = await _storageProvider.UploadImage(request.File, cancellationToken);

    _logger.LogInformation(
      "New avatar uploaded to storage for user {UserId}. BlobName: {BlobName}",
      user.Id, uploadedImage.Name);

    try
    {
      bool isNewAvatar = user.UserAvatar is null;

      if (isNewAvatar)
      {
        var userAvatar =
          UserAvatar.Create(uploadedImage.Uri, request.UserId, uploadedImage.Name, uploadedImage.ContentType);

        await _repository.AddUserAvatar(userAvatar, cancellationToken);
      }
      else
      {
        user.UserAvatar!.UpdateFileName(uploadedImage.Name);
        user.UserAvatar!.UpdateUrl(uploadedImage.Uri);
        _repository.UpdateUserAvatar(user.UserAvatar, cancellationToken);
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);

      if (!isNewAvatar && previewImageName is not null)
      {
        await DeletePreviousImageSafelyAsync(previewImageName, user.Id, cancellationToken);
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

      await RollbackUploadSafelyAsync(uploadedImage.Name, user.Id, cancellationToken);

      return Result<UpdateAvatarResponse>.Failure(
        Error.Failure("Failed to update avatar. Please try again."));
    }
  }

  private async Task DeletePreviousImageSafelyAsync(
    string imageName,
    Guid userId,
    CancellationToken ct
  )
  {
    try
    {
      await _storageProvider.DeleteImage(imageName, ct);

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
    Guid userId,
    CancellationToken ct
  )
  {
    try
    {
      await _storageProvider.DeleteImage(imageName, ct);

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
