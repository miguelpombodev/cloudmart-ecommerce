using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using Cloudmart.Contracts.Messaging.Interfaces.Notifications;
using FluentAssertions;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.Register;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.Registry;

namespace Identity.Tests.Application.Users.Register;

public class RegisterUserCommandHandlerTests
{
  private readonly RegisterUserCommandHandler _commandHandler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<IRoleRepository> _roleRepositoryMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  private readonly Mock<IPublishEndpoint> _publishEndpointMock;

  private readonly Mock<ResiliencePipelineProvider<string>> _pipeline;

  public RegisterUserCommandHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _roleRepositoryMock = new Mock<IRoleRepository>();
    _uowMock = new Mock<IUnitOfWork>();
    _publishEndpointMock = new Mock<IPublishEndpoint>();
    _pipeline = new Mock<ResiliencePipelineProvider<string>>();

    var logger = new Mock<ILogger<RegisterUserCommandHandler>>();

    _commandHandler = new RegisterUserCommandHandler(
      _repositoryMock.Object,
      _roleRepositoryMock.Object,
      _uowMock.Object,
      _pipeline.Object,
      _publishEndpointMock.Object,
      logger.Object);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldReturnSuccess()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    // Act
    Result<RegisterUserResponse> result =
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldCallAddAsyncOnRepository()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _repositoryMock.Verify(
      r => r.AddAsync(
        It.Is<User>(u =>
          u.Email.Address == command.Email &&
          u.Name.FirstName == command.FirstName &&
          u.Name.LastName == command.LastName),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldCommitUnitOfWork()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _uowMock.Verify(
      r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldFindCustomerRole()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _roleRepositoryMock.Verify(
      r => r.FindRoleByName(
        "Customer",
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldCreateUserWithCustomerRole()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    User? capturedUser = null;

    _repositoryMock
      .Setup(r => r.AddAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()))
      .Callback<User, CancellationToken>((user, _) => capturedUser = user);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    capturedUser.Should().NotBeNull();
    capturedUser!.Email.Address.Should().Be(command.Email);
    capturedUser.Name.FirstName.Should().Be(command.FirstName);
    capturedUser.Name.LastName.Should().Be(command.LastName);
    capturedUser.Role.Should().Be(role);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldPublishWelcomeNotification()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _publishEndpointMock.Verify(
      p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldPublishNotificationToRegisteredEmail()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    INotificationRequest? publishedNotification = null;

    _publishEndpointMock
      .Setup(p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()))
      .Callback<INotificationRequest, CancellationToken>((notification, _) => publishedNotification = notification);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    publishedNotification.Should().NotBeNull();
    publishedNotification!.Recipient.Should().Be(command.Email);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldPublishWelcomeTemplate()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    INotificationRequest? publishedNotification = null;

    _publishEndpointMock
      .Setup(p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()))
      .Callback<INotificationRequest, CancellationToken>((notification, _) => publishedNotification = notification);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    publishedNotification.Should().NotBeNull();
    publishedNotification!.Template.Should().Be("welcome");
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldPublishExpectedNotificationData()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    INotificationRequest? publishedNotification = null;

    _publishEndpointMock
      .Setup(p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()))
      .Callback<INotificationRequest, CancellationToken>((notification, _) => publishedNotification = notification);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    publishedNotification.Should().NotBeNull();

    publishedNotification!.Data["name"]
      .Should()
      .Be(command.FirstName);

    publishedNotification.Data["subject"]
      .Should()
      .Be("Welcome to CloudMart!");

    publishedNotification.Data["loginUrl"]
      .Should()
      .Be("https://cloudmart.example.com/login");
  }

  [Fact]
  public async Task Handle_WithAlreadyRegisteredEmail_ShouldReturnConflictError()
  {
    // Arrange
    User existingUser = new UserBuilder()
      .WithEmail("joao@example.com")
      .Build();

    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync(existingUser);

    // Act
    Result<RegisterUserResponse> result =
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Description.Should().Be("User already registered!");
    result.Error.InternalCode.Should().Be("Error.Conflict");
    result.Error.StatusCode.Should().Be(409);
  }

  [Fact]
  public async Task Handle_WithAlreadyRegisteredEmail_ShouldNeverCallAddAsync()
  {
    // Arrange
    User existingUser = new UserBuilder()
      .WithEmail("joao@example.com")
      .Build();

    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync(existingUser);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _repositoryMock.Verify(
      r => r.AddAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WithAlreadyRegisteredEmail_ShouldNeverCommit()
  {
    // Arrange
    User existingUser = new UserBuilder()
      .WithEmail("joao@example.com")
      .Build();

    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync(existingUser);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _uowMock.Verify(
      u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WithAlreadyRegisteredEmail_ShouldNeverPublishNotification()
  {
    // Arrange
    User existingUser = new UserBuilder()
      .WithEmail("joao@example.com")
      .Build();

    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync(existingUser);

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _publishEndpointMock.Verify(
      p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldQueryRepositoryUsingExactEmailFromCommand()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(It.IsAny<string>(), CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName(
        "Customer",
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(Role.Create("Customer"));

    // Act
    await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    _repositoryMock.Verify(
      r => r.FindByEmail("joao@example.com", CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldPassCancellationTokenToDependencies()
  {
    // Arrange
    using var cts = new CancellationTokenSource();
    CancellationToken ct = cts.Token;

    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName("Customer", ct))
      .ReturnsAsync(role);

    // Act
    await _commandHandler.Handle(command, ct);

    // Assert
    _roleRepositoryMock.Verify(
      r => r.FindRoleByName("Customer", ct),
      Times.Once);

    _repositoryMock.Verify(
      r => r.AddAsync(
        It.IsAny<User>(),
        ct),
      Times.Once);

    _uowMock.Verify(
      r => r.SaveChangesAsync(ct),
      Times.Once);

    _publishEndpointMock.Verify(
      r => r.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        ct),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    var exception = new InvalidOperationException("Database error");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ThrowsAsync(exception);

    // Act
    Func<Task> act = async () =>
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should()
      .ThrowAsync<InvalidOperationException>()
      .WithMessage("Database error");

    _roleRepositoryMock.Verify(
      r => r.FindRoleByName(
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenRoleRepositoryThrows_ShouldNotAddUser()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    var exception = new InvalidOperationException("Role not found");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName(
        "Customer",
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(exception);

    // Act
    Func<Task> act = async () =>
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should()
      .ThrowAsync<InvalidOperationException>()
      .WithMessage("Role not found");

    _repositoryMock.Verify(
      r => r.AddAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(
      r => r.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Never);

    _publishEndpointMock.Verify(
      r => r.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenSaveChangesThrows_ShouldNotPublishNotification()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName(
        "Customer",
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    _uowMock
      .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Database commit failed"));

    // Act
    Func<Task> act = async () =>
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should()
      .ThrowAsync<InvalidOperationException>()
      .WithMessage("Database commit failed");

    _repositoryMock.Verify(
      r => r.AddAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()),
      Times.Once);

    _publishEndpointMock.Verify(
      r => r.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenPublishNotificationThrows_ShouldPropagateException()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName(
        "Customer",
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    _publishEndpointMock
      .Setup(p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("RabbitMQ unavailable"));

    // Act
    Func<Task> act = async () =>
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should()
      .ThrowAsync<InvalidOperationException>()
      .WithMessage("RabbitMQ unavailable");

    _repositoryMock.Verify(
      r => r.AddAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()),
      Times.Once);

    _uowMock.Verify(
      r => r.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldPersistUserBeforePublishingNotification()
  {
    // Arrange
    var command = new RegisterUserCommand(
      "João",
      "Silva",
      "joao@example.com",
      "Senha@123");

    Role role = Role.Create("Customer");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email, CancellationToken.None))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(r => r.FindRoleByName(
        "Customer",
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(role);

    var sequence = new MockSequence();

    _repositoryMock
      .InSequence(sequence)
      .Setup(r => r.AddAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()));

    _uowMock
      .InSequence(sequence)
      .Setup(r => r.SaveChangesAsync(
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(1);

    _publishEndpointMock
      .InSequence(sequence)
      .Setup(p => p.Publish<INotificationRequest>(
        It.IsAny<INotificationRequest>(),
        It.IsAny<CancellationToken>()));

    // Act
    Result<RegisterUserResponse> result =
      await _commandHandler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }
}
