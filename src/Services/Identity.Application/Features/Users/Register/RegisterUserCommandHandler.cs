using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Events.Notifications;
using BuildingBlocks.Infrastructure;
using Cloudmart.Contracts.Messaging.Interfaces.Notifications;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using Identity.Domain.ValueObject;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Users.Register;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
  private const string WelcomeTemplateName = "welcome";

  private readonly ILogger<RegisterUserCommandHandler> _logger;

  private readonly IUserRepository _repository;

  private readonly IRoleRepository _roleRepository;

  private readonly IPublishEndpoint _publishEndpoint;

  private readonly IUnitOfWork _uow;

  public RegisterUserCommandHandler(
    IUserRepository repository,
    IRoleRepository roleRepository,
    IUnitOfWork uow,
    IPublishEndpoint publishEndpoint,
    ILogger<RegisterUserCommandHandler> logger)
  {
    _repository = repository;
    _roleRepository = roleRepository;
    _uow = uow;
    _publishEndpoint = publishEndpoint;
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
    var passwordResult = Password.CreateWithNoProvider(request.Password);
    var emailResult = Email.Create(request.Email);

    Role role = await _roleRepository.FindRoleByName("Customer", cancellationToken);

    Result<User> userResult = User.Create(
      completeNameResult,
      emailResult,
      passwordResult,
      role);

    await _repository.AddAsync(userResult.Value, cancellationToken);
    await _uow.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Sending notification for email {UserEmail} ", userResult.Value.Email.Address);

    await _publishEndpoint.Publish<INotificationRequest>(new NotificationRequested()
    {
      Recipient = emailResult.Address,
      Template = WelcomeTemplateName,
      Data =
      {
        { "name", userResult.Value.Name.FirstName},
        {"subject", "Welcome to CloudMart!"},
        {"loginUrl", "https://cloudmart.example.com/login"}
      }
    }, cancellationToken);

    return userResult.Value.Adapt<RegisterUserResponse>();
  }
}
