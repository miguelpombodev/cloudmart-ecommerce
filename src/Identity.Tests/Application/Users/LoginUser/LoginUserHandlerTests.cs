using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using FluentAssertions;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.Login;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Tests.Application.Users.LoginUser;

public class LoginUserHandlerTests
{
  private readonly LoginCommandHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<ITokenService> _tokenServiceMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  public LoginUserHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _uowMock = new Mock<IUnitOfWork>();
    _tokenServiceMock = new Mock<ITokenService>();

    var logger = new Mock<ILogger<LoginCommandHandler>>();

    _handler = new LoginCommandHandler(
      _repositoryMock.Object,
      _tokenServiceMock.Object,
      _uowMock.Object,
      logger.Object);
  }

  [Fact]
  public async Task Handle_WithValidCredentials_ShouldReturnTokens()
  {
    // Arrange
    DateTimeOffset before = DateTimeOffset.UtcNow;
    var role = Role.Create("test role");

    User existingUser = new UserBuilder().WithEmail("john_doe@test.com").WithPassword("Password@123!").WithRole(role)
      .Build();

    var refreshToken = RefreshToken.Create(existingUser.Id, new string('*', 64), DateTimeOffset.UtcNow);

    var command = new LoginCommand("john_doe@test.com", "Password@123!", "127.0.0.1");

    var tokenResult = new TokenResult(
      "jwt-token",
      DateTime.UtcNow.AddMinutes(15),
      "test-jti",
      "Bearer"
    );

    // Act
    _repositoryMock.Setup(r => r.FindByEmail(command.Email)).ReturnsAsync(existingUser);
    _repositoryMock.Setup(r => r.AddRefreshToken(refreshToken, CancellationToken.None));
    _tokenServiceMock.Setup(t => t.GenerateAccessToken(existingUser)).Returns(tokenResult);
    _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(new string('*', 64));
    _tokenServiceMock.Setup(t => t.HashRefreshToken(new string('*', 64))).Returns("hashed-refresh-token");

    Result<LoginResponse> result = await _handler.Handle(command, CancellationToken.None);

    DateTimeOffset after = DateTimeOffset.UtcNow;

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.AccessToken.Should().Be("jwt-token");
    result.Value.RefreshToken.Should().HaveLength(64);
    result.Value.TokenType.Should().Be("Bearer");
    result.Value.ExpiresAt.Should().BeOnOrAfter(before.AddMinutes(2));
    result.Value.ExpiresAt.Should().BeOnOrBefore(after.AddMinutes(2));

    _repositoryMock.Verify(x => x.AddRefreshToken(It.IsAny<RefreshToken>(), CancellationToken.None), Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldReturnUnauthorized()
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      "Password@123!",
      "127.0.0.1");

    _repositoryMock
      .Setup(x => x.FindByEmail(command.Email))
      .ReturnsAsync((User?)null);

    // Act
    Result<LoginResponse> result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    _repositoryMock.Verify(x =>
        x.AddRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(x =>
        x.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
  {
    // Arrange
    var role = Role.Create("Admin");

    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .WithPassword("Password@123!")
      .WithRole(role)
      .Build();

    var command = new LoginCommand(
      user.Email.Address,
      "WrongPassword",
      "127.0.0.1");

    _repositoryMock
      .Setup(x => x.FindByEmail(command.Email))
      .ReturnsAsync(user);

    // Act
    Result<LoginResponse> result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    _repositoryMock.Verify(x =>
        x.AddRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenUserIsInactive_ShouldReturnUnauthorized()
  {
    // Arrange
    var role = Role.Create("Admin");

    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .WithPassword("Password@123!")
      .WithRole(role)
      .Build();

    user.Revoke();

    var command = new LoginCommand(
      user.Email.Address,
      "Password@123!",
      "127.0.0.1");

    _repositoryMock
      .Setup(x => x.FindByEmail(command.Email))
      .ReturnsAsync(user);

    // Act
    Result<LoginResponse> result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    _repositoryMock.Verify(x =>
        x.AddRefreshToken(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(x =>
        x.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }
}
