using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Identity.Application.Features.Users.InactivateUser;

public sealed class InactivateUserCommandHandler : ICommandHandler<InactivateUserCommand, Result<Unit>>
{
  private readonly ILogger<InactivateUserCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly IUnitOfWork _unitOfWork;

  private readonly ResiliencePipeline _databasePipeline;

  public InactivateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<InactivateUserCommandHandler> logger)
  {
    _logger = logger;
    _unitOfWork = unitOfWork;
    _repository = repository;
    _databasePipeline = pipelineProvider.GetPipeline("database-operations");
  }

  public async Task<Result<Unit>> Handle(InactivateUserCommand request, CancellationToken cancellationToken)
  {
    User? checkUser = await _databasePipeline.ExecuteAsync(async ct => await _repository.FindByIdAsync(
        request.UserId, ct
      )
    );

    if (checkUser is null)
    {
      _logger.LogWarning(
        "User status update attempted for non-existent user {UserId}",
        request.UserId);

      return Result<Unit>.Failure(Error.Conflict("User not registered!"));
    }

    checkUser.InactivateUser();

    await _databasePipeline.ExecuteAsync(async ct =>
    {
      _repository.UpdateAsync(checkUser, ct);
      await _unitOfWork.SaveChangesAsync(ct);
    });

    _logger.LogInformation("User {UserId} inactivated successfully", checkUser.Id);

    Unit response = new();

    return Result<Unit>.Success(response);
  }
}
