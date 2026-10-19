using BuildingBlocks.Abstractions;
using BuildingBlocks.Authorization;
using BuildingBlocks.CQRS;
using BuildingBlocks.Events.Notifications.Factories.WelcomeEmail;
using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using Identity.Domain.ValueObject;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Identity.Application.Features.Users.Register;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
  private readonly ResiliencePipeline _databasePipeline;

  private readonly ILogger<RegisterUserCommandHandler> _logger;

  private readonly IPublishEndpoint _publishEndpoint;

  private readonly IUserRepository _repository;

  private readonly IRoleRepository _roleRepository;


  private readonly IUnitOfWork _uow;

  public RegisterUserCommandHandler(
    IUserRepository repository,
    IRoleRepository roleRepository,
    IUnitOfWork uow,
    ResiliencePipelineProvider<string> pipelineProvider,
    IPublishEndpoint publishEndpoint,
    ILogger<RegisterUserCommandHandler> logger)
  {
    _repository = repository;
    _roleRepository = roleRepository;
    _uow = uow;
    _publishEndpoint = publishEndpoint;
    _logger = logger;
    _databasePipeline = pipelineProvider.GetPipeline("database-operations");
  }

  public async Task<Result<RegisterUserResponse>> Handle(
    RegisterUserCommand request,
    CancellationToken cancellationToken)
  {
    User? checkUserExists =
      await _databasePipeline.ExecuteAsync(async ct => await _repository.FindByEmail(request.Email, ct),
        cancellationToken);

    if (checkUserExists is not null)
    {
      _logger.LogWarning(
        "There was an attempt create a registered user in database. Email: {UserEmail}",
        checkUserExists.RetrieveMaskedEmail());

      return Result<RegisterUserResponse>.Failure(Error.Conflict("User already registered!"));
    }

    User user = await CreateUserAsync(request, cancellationToken);

    await _databasePipeline.ExecuteAsync(async ct =>
      {
        await _repository.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
      },
      cancellationToken
    );

    _logger.LogInformation("Sending notification for email {UserEmail} ", user.RetrieveMaskedEmail());

    var notification = new WelcomeEmailData(
      user.Name.FirstName,
      user.Email.Address
    );

    await _publishEndpoint.Publish(
      WelcomeEmailNotification.Create(notification),
      cancellationToken);

    return user.Adapt<RegisterUserResponse>();
  }

  private async Task<User> CreateUserAsync(RegisterUserCommand request, CancellationToken cancel)
  {
    var completeNameResult = CompleteName.Create(request.FirstName, request.LastName);
    var passwordResult = Password.CreateWithNoProvider(request.Password);
    var emailResult = Email.Create(request.Email);

    Role role = await _databasePipeline.ExecuteAsync(
      async ct => await _roleRepository.FindRoleByName(nameof(RoleType.Customer), ct),
      cancel);

    Result<User> userResult = User.Create(
      completeNameResult,
      emailResult,
      passwordResult,
      role);

    return userResult.Value;
  }
}
