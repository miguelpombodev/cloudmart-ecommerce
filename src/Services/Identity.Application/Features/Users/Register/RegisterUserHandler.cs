using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Domain.ValueObject;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.Register;

public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
  private readonly ILogger<RegisterUserHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly IUnitOfWork _uow;

  public RegisterUserHandler(IUserRepository repository, IUnitOfWork uow, ILogger<RegisterUserHandler> logger)
  {
    _repository = repository;
    _uow = uow;
    _logger = logger;
  }

  public async Task<Result<RegisterUserResponse>> Handle(
    RegisterUserCommand request,
    CancellationToken cancellationToken)
  {
    User? checkUserExists = await _repository.FindByEmail(request.Email);

    if (checkUserExists is not null)
    {
      _logger.LogWarning(
        "There was an attempt create a registered user in database. Email: {UserEmail}",
        checkUserExists.Email.Address);

      return Result<RegisterUserResponse>.Failure(Error.Conflict("User already registered!"));
    }

    var completeNameResult = CompleteName.Create(request.FirstName, request.LastName);
    var passwordResult = Password.Create(request.Password);
    var emailResult = Email.Create(request.Email);

    var role = Role.Create("Customer");

    Result<User> userResult = User.Create(
      completeNameResult,
      emailResult,
      passwordResult,
      role);

    await _repository.AddAsync(userResult.Value, cancellationToken);
    await _uow.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Created user with email {UserEmail} ", userResult.Value.Email.Address);

    return userResult.Value.Adapt<RegisterUserResponse>();
  }
}
