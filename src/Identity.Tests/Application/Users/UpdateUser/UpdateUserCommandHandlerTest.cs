using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using FluentAssertions;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.UpdateUser;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.Registry;

namespace Identity.Tests.Application.Users.UpdateUser;

public class UpdateUserCommandHandlerTests
{
  private readonly UpdateUserCommandHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  private readonly Mock<ResiliencePipelineProvider<string>> _pipeline;

  public UpdateUserCommandHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _uowMock = new Mock<IUnitOfWork>();
    _pipeline = new Mock<ResiliencePipelineProvider<string>>();

    var logger = new Mock<ILogger<UpdateUserCommandHandler>>();

    _handler = new UpdateUserCommandHandler(
      _repositoryMock.Object,
      logger.Object,
      _pipeline.Object,
      _uowMock.Object);
  }

  [Fact]
  public async Task Handle_WithValidData_ShouldUpdateUserAndSaveChanges()
  {
    // Arrange
    var role = Role.Create("Admin");

    User user = new UserBuilder()
      .WithEmail("old_email@test.com")
      .WithName("Old", "Name")
      .WithRole(role)
      .Build();

    var command = new UpdateUserCommand(
      user.Id,
      "new_email@test.com",
      "John",
      "Doe");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        command.UserId,
        CancellationToken.None))
      .ReturnsAsync(user);

    // Act
    Result<Unit> result = await _handler.Handle(
      command,
      CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(Unit.Value);

    user.Email.Address.Should().Be("new_email@test.com");
    user.Name.FirstName.Should().Be("John");
    user.Name.LastName.Should().Be("Doe");

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        command.UserId,
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(user, CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldReturnNotFound()
  {
    // Arrange
    var userId = Guid.NewGuid();

    var command = new UpdateUserCommand(
      userId,
      "new_email@test.com",
      "John",
      "Doe");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        userId,
        CancellationToken.None))
      .ReturnsAsync((User?)null);

    // Act
    Result<Unit> result = await _handler.Handle(
      command,
      CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.Should().Be(
      Error.NotFound("User not registered"));

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        userId,
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(It.IsAny<User>(), CancellationToken.None),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WithValidData_ShouldOnlyUpdateRequestedFields()
  {
    // Arrange
    var role = Role.Create("Admin");

    User user = new UserBuilder()
      .WithEmail("old_email@test.com")
      .WithName("Old", "Name")
      .WithRole(role)
      .Build();

    Role originalRole = user.Role;
    Guid originalUserId = user.Id;

    var command = new UpdateUserCommand(
      user.Id,
      "new_email@test.com",
      "John",
      "Doe");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        command.UserId,
        CancellationToken.None))
      .ReturnsAsync(user);

    // Act
    Result<Unit> result = await _handler.Handle(
      command,
      CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    user.Id.Should().Be(originalUserId);
    user.Role.Should().BeSameAs(originalRole);

    user.Email.Address.Should().Be("new_email@test.com");
    user.Name.FirstName.Should().Be("John");
    user.Name.LastName.Should().Be("Doe");

    _repositoryMock.Verify(
      x => x.UpdateAsync(It.Is<User>(u =>
        u.Id == command.UserId &&
        u.Email.Address == command.Email &&
        u.Name.FirstName == command.FirstName &&
        u.Name.LastName == command.LastName), CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldPassCancellationTokenToRepositoryAndUnitOfWork()
  {
    // Arrange
    var role = Role.Create("Admin");

    User user = new UserBuilder()
      .WithEmail("old_email@test.com")
      .WithName("Old", "Name")
      .WithRole(role)
      .Build();

    using var cts = new CancellationTokenSource();
    CancellationToken ct = cts.Token;

    var command = new UpdateUserCommand(
      user.Id,
      "new_email@test.com",
      "John",
      "Doe");

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        command.UserId,
        ct))
      .ReturnsAsync(user);

    // Act
    Result<Unit> result = await _handler.Handle(
      command,
      ct);

    // Assert
    result.IsSuccess.Should().BeTrue();

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        command.UserId,
        ct),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(ct),
      Times.Once);
  }
}
