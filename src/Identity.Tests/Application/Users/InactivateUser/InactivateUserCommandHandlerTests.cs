using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using FluentAssertions;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.InactivateUser;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.Registry;

namespace Identity.Tests.Application.Users.InactivateUser;

public class InactivateUserCommandHandlerTests
{
  private readonly InactivateUserCommandHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  private readonly Mock<ResiliencePipelineProvider<string>> _pipeline;

  public InactivateUserCommandHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _uowMock = new Mock<IUnitOfWork>();
    _pipeline = new Mock<ResiliencePipelineProvider<string>>();

    var logger = new Mock<ILogger<InactivateUserCommandHandler>>();

    _handler = new InactivateUserCommandHandler(
      _repositoryMock.Object,
      _uowMock.Object,
      _pipeline.Object,
      logger.Object);
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldReturnConflict()
  {
    // Arrange
    Guid userId = Guid.NewGuid();

    var command = new InactivateUserCommand(userId);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        userId,
        CancellationToken.None))
      .ReturnsAsync((User?)null);

    // Act
    Result<Unit> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.StatusCode.Should().Be(409);
    result.Error.Description.Should().Be("User not registered!");

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        userId,
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(
        It.IsAny<User>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenUserExists_ShouldInactivateUser()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    user.IsActive.Should().BeTrue();

    var command = new InactivateUserCommand(user.Id);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None))
      .ReturnsAsync(user);

    _repositoryMock
      .Setup(x => x.UpdateAsync(
        It.IsAny<User>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(
        CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<Unit> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.Should().Be(Unit.Value);

    user.IsActive.Should().BeFalse();

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateAsync(
        It.Is<User>(updatedUser =>
          updatedUser.Id == user.Id &&
          !updatedUser.IsActive),
        CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserExists_ShouldUpdateUserAndSaveChanges()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    var command = new InactivateUserCommand(user.Id);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(
        user.Id,
        CancellationToken.None))
      .ReturnsAsync(user);

    _repositoryMock
      .Setup(x => x.UpdateAsync(
        It.IsAny<User>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(
        CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<Unit> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    _repositoryMock.Verify(
      x => x.UpdateAsync(
        It.Is<User>(updatedUser =>
          updatedUser.Id == user.Id &&
          !updatedUser.IsActive),
        CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        CancellationToken.None),
      Times.Once);
  }
}
