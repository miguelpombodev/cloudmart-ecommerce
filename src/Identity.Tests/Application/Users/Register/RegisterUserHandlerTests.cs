using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using FluentAssertions;
using Identity.Application.Abstractions;
using Identity.Application.Features.Users.Register;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Tests.Application.Users.Register;

public class RegisterUserHandlerTests
{
  private readonly RegisterUserHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  public RegisterUserHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _uowMock = new Mock<IUnitOfWork>();

    var logger = new Mock<ILogger<RegisterUserHandler>>();

    _handler = new RegisterUserHandler(
      _repositoryMock.Object,
      _uowMock.Object,
      logger.Object);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldCallAddAsyncOnRepository()
  {
    var command = new RegisterUserCommand("João", "Silva", "joao@example.com", "Senha@123");

    _repositoryMock.Setup(r => r.FindByEmail(command.Email)).ReturnsAsync((User?)null);

    await _handler.Handle(command, CancellationToken.None);

    _repositoryMock.Verify(
      r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithNewEmail_ShouldCommitUnitOfWork()
  {
    var command = new RegisterUserCommand("João", "Silva", "joao@example.com", "Senha@123");

    _repositoryMock.Setup(r => r.FindByEmail(command.Email)).ReturnsAsync((User?)null);

    await _handler.Handle(command, CancellationToken.None);

    _uowMock.Verify(
      r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithAlreadyRegisteredEmail_ShouldReturnConflictError()
  {
    User existingUser = new UserBuilder()
      .WithEmail("joao@example.com")
      .Build();

    var command = new RegisterUserCommand("João", "Silva", "joao@example.com", "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email))
      .ReturnsAsync(existingUser);

    // Act
    Result<RegisterUserResponse> result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Description.Should().Be("User already registered!");
    result.Error.InternalCode.Should().Be("Error.Conflict");
    result.Error.StatusCode.Should().Be(409);
  }

  [Fact]
  public async Task Handle_WithAlreadyRegisteredEmail_ShouldNeverCallAddAsync()
  {
    User existingUser = new UserBuilder()
      .WithEmail("joao@example.com")
      .Build();

    var command = new RegisterUserCommand("João", "Silva", "joao@example.com", "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(command.Email))
      .ReturnsAsync(existingUser);

    await _handler.Handle(command, CancellationToken.None);

    _repositoryMock.Verify(
      r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(
      u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldQueryRepositoryUsingExactEmailFromCommand()
  {
    var command = new RegisterUserCommand("João", "Silva", "joao@example.com", "Senha@123");

    _repositoryMock
      .Setup(r => r.FindByEmail(It.IsAny<string>()))
      .ReturnsAsync((User?)null);

    await _handler.Handle(command, CancellationToken.None);

    _repositoryMock.Verify(
      r => r.FindByEmail("joao@example.com"),
      Times.Once);
  }
}
