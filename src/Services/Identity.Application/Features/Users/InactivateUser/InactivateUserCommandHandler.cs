using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.InactivateUser;

public sealed class InactivateUserCommandHandler : ICommandHandler<InactivateUserCommand, Result<Unit>>
{
  private readonly IUserRepository _repository;

  private readonly ILogger<InactivateUserCommandHandler> _logger;

  private readonly IUnitOfWork _unitOfWork;

  public InactivateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<InactivateUserCommandHandler> logger)
  {
    _logger = logger;
    _unitOfWork = unitOfWork;
    _repository = repository;
  }

  public async Task<Result<Unit>> Handle(InactivateUserCommand request, CancellationToken cancellationToken)
  {
    User? checkUser = await _repository.FindByIdAsync(request.UserId, cancellationToken);

    if (checkUser is null)
    {
      _logger.LogWarning(
        "User status update attempted for non-existent user {UserId}",
        request.UserId);

      return Result<Unit>.Failure(Error.Conflict("User not registered!"));
    }

    checkUser.InactivateUser();

    _repository.UpdateAsync(checkUser);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User {UserId} inactivated successfully", checkUser.Id);

    Unit response = new();

    return Result<Unit>.Success(response);
  }
}
