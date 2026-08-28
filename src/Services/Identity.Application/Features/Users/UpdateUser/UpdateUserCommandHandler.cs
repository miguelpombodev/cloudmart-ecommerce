using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.UpdateUser;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<Unit>>
{
  private readonly ILogger<UpdateUserCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly IUnitOfWork _unitOfWork;

  public UpdateUserCommandHandler(
    IUserRepository repository,
    ILogger<UpdateUserCommandHandler> logger,
    IUnitOfWork unitOfWork)
  {
    _repository = repository;
    _logger = logger;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<Unit>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    User? user = await _repository.FindByIdAsync(request.UserId, cancellationToken);

    if (user is null)
    {
      _logger.LogWarning(
        "User data update attempted for non-existent user {UserId}",
        request.UserId);

      return Result<Unit>.Failure(Error.NotFound("User not registered"));
    }

    User newUser = SetNewValues(user, request);

    _logger.LogInformation("New information set for User {UserId}: Data: {NewUserInformations}", newUser.Id, newUser);

    _repository.UpdateAsync(newUser, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

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
