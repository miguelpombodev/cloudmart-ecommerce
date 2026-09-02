using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Identity.Application.Features.Users.UpdateUser;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<Unit>>
{
  private readonly ILogger<UpdateUserCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly ResiliencePipeline _databasePipeline;

  private readonly IUnitOfWork _unitOfWork;

  public UpdateUserCommandHandler(
    IUserRepository repository,
    ILogger<UpdateUserCommandHandler> logger,
    ResiliencePipelineProvider<string> pipelineProvider,
    IUnitOfWork unitOfWork)
  {
    _repository = repository;
    _logger = logger;
    _databasePipeline = pipelineProvider.GetPipeline("database-operations");
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<Unit>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    User? user = await _databasePipeline.ExecuteAsync(async ct => await _repository.FindByIdAsync(
        request.UserId,
        ct
      )
    );

    if (user is null)
    {
      _logger.LogWarning(
        "User data update attempted for non-existent user {UserId}",
        request.UserId);

      return Result<Unit>.Failure(Error.NotFound("User not registered"));
    }

    User newUser = SetNewValues(user, request);

    _logger.LogInformation("New information set for User {UserId}: Data: {NewUserInformations}", newUser.Id, newUser);

    await _databasePipeline.ExecuteAsync(async ct =>
    {
      _repository.UpdateAsync(newUser, ct);
      await _unitOfWork.SaveChangesAsync(ct);
    });

    _logger.LogInformation("New information for User {UserId} were updated successfully", newUser.Id);

    return Result<Unit>.Success(new Unit());
  }

  private static User SetNewValues(User user, UpdateUserCommand request)
  {
    user.SetEmail(request.Email);
    user.SetName(request.FirstName, request.LastName);

    return user;
  }
}
