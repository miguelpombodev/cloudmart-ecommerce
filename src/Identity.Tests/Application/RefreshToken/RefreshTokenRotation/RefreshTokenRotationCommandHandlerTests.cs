using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using FluentAssertions;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.RefreshTokens.Rotation;
using Identity.Domain.Entities;
using Identity.Tests.Domain.Builder;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Tests.Application.RefreshTokens;

public class RefreshTokenRotationCommandHandlerTests
{
  private readonly RefreshTokenRotationCommandHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;
  private readonly Mock<ITokenService> _tokenServiceMock;
  private readonly Mock<IUnitOfWork> _uowMock;
  private readonly Mock<ILogger<RefreshTokenRotationCommandHandler>> _loggerMock;

  public RefreshTokenRotationCommandHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _tokenServiceMock = new Mock<ITokenService>();
    _uowMock = new Mock<IUnitOfWork>();
    _loggerMock = new Mock<ILogger<RefreshTokenRotationCommandHandler>>();

    _handler = new RefreshTokenRotationCommandHandler(
      _repositoryMock.Object,
      _tokenServiceMock.Object,
      _uowMock.Object,
      _loggerMock.Object);
  }

  [Fact]
  public async Task Handle_WithValidRefreshToken_ShouldRotateToken()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    const string rawOldRefreshToken = "old-refresh-token";
    const string hashedOldRefreshToken = "hashed-old-refresh-token";
    const string rawNewRefreshToken = "new-refresh-token";
    const string hashedNewRefreshToken = "hashed-new-refresh-token";

    RefreshToken oldToken = RefreshToken.Create(
      user.Id,
      hashedOldRefreshToken,
      DateTimeOffset.UtcNow.AddMinutes(10));

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawOldRefreshToken);

    var tokenResult = new TokenResult(
      "new-access-token",
      DateTime.UtcNow.AddMinutes(15),
      "new-jti",
      "Bearer");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawOldRefreshToken))
      .Returns(hashedOldRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedOldRefreshToken))
      .ReturnsAsync(oldToken);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(user))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns(rawNewRefreshToken);

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawNewRefreshToken))
      .Returns(hashedNewRefreshToken);

    _repositoryMock
      .Setup(x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None));

    _repositoryMock
      .Setup(x => x.UpdateRefreshToken(oldToken));

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<RefreshTokenRotationResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.AccessToken
      .Should()
      .Be("new-access-token");

    result.Value.RefreshToken
      .Should()
      .Be(rawNewRefreshToken);

    oldToken.IsRevoked.Should().BeTrue();

    _tokenServiceMock.Verify(
      x => x.HashRefreshToken(rawOldRefreshToken),
      Times.Once);

    _repositoryMock.Verify(
      x => x.RetrieveLastOldRefreshToken(hashedOldRefreshToken),
      Times.Once);

    _repositoryMock.Verify(
      x => x.FindByIdAsync(user.Id, CancellationToken.None),
      Times.Once);

    _tokenServiceMock.Verify(
      x => x.GenerateAccessToken(user),
      Times.Once);

    _tokenServiceMock.Verify(
      x => x.GenerateRefreshToken(),
      Times.Once);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.Is<RefreshToken>(token =>
          token.UserId == user.Id &&
          token.Token == hashedNewRefreshToken),
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.UpdateRefreshToken(oldToken),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenRefreshTokenDoesNotExist_ShouldReturnUnauthorized()
  {
    // Arrange
    const string rawRefreshToken = "unknown-refresh-token";
    const string hashedRefreshToken = "hashed-unknown-token";

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawRefreshToken);

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawRefreshToken))
      .Returns(hashedRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedRefreshToken))
      .ReturnsAsync((RefreshToken?)null);

    // Act
    Result<RefreshTokenRotationResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.Description
      .Should()
      .Be("Refresh Token does not exist");

    result.Error.StatusCode.Should().Be(401);

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.UpdateRefreshToken(
        It.IsAny<RefreshToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenStoredTokenDoesNotMatchHashedToken_ShouldReturnUnauthorized()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    const string rawRefreshToken = "old-refresh-token";
    const string hashedRefreshToken = "hashed-refresh-token";
    const string differentStoredToken = "different-hashed-token";

    RefreshToken oldToken = RefreshToken.Create(
      user.Id,
      differentStoredToken,
      DateTimeOffset.UtcNow.AddMinutes(10));

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawRefreshToken);

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawRefreshToken))
      .Returns(hashedRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedRefreshToken))
      .ReturnsAsync(oldToken);

    // Act
    Result<RefreshTokenRotationResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.Description
      .Should()
      .Be("Refresh Token must be valid");

    result.Error.StatusCode.Should().Be(401);

    _repositoryMock.Verify(
      x => x.FindByIdAsync(
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.UpdateRefreshToken(
        It.IsAny<RefreshToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldReturnUnauthorized()
  {
    // Arrange
    const string rawRefreshToken = "old-refresh-token";
    const string hashedRefreshToken = "hashed-old-refresh-token";

    Guid userId = Guid.NewGuid();

    RefreshToken oldToken = RefreshToken.Create(
      userId,
      hashedRefreshToken,
      DateTimeOffset.UtcNow.AddMinutes(10));

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawRefreshToken);

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawRefreshToken))
      .Returns(hashedRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedRefreshToken))
      .ReturnsAsync(oldToken);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(userId, CancellationToken.None))
      .ReturnsAsync((User?)null);

    // Act
    Result<RefreshTokenRotationResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.Description
      .Should()
      .Be("User's Refresh Token must be valid");

    result.Error.StatusCode.Should().Be(401);

    oldToken.IsRevoked.Should().BeTrue();

    _repositoryMock.Verify(
      x => x.FindByIdAsync(userId, CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.UpdateRefreshToken(
        It.IsAny<RefreshToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WithValidToken_ShouldRevokeOldToken()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    const string rawRefreshToken = "old-refresh-token";
    const string hashedRefreshToken = "hashed-old-refresh-token";

    RefreshToken oldToken = RefreshToken.Create(
      user.Id,
      hashedRefreshToken,
      DateTimeOffset.UtcNow.AddMinutes(10));

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawRefreshToken);

    var tokenResult = new TokenResult(
      "new-access-token",
      DateTime.UtcNow.AddMinutes(15),
      "new-jti",
      "Bearer");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawRefreshToken))
      .Returns(hashedRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedRefreshToken))
      .ReturnsAsync(oldToken);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(user))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns("new-refresh-token");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken("new-refresh-token"))
      .Returns("hashed-new-refresh-token");

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    oldToken.IsRevoked.Should().BeTrue();

    _repositoryMock.Verify(
      x => x.UpdateRefreshToken(oldToken),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithValidToken_ShouldCreateNewRefreshTokenUsingHashedValue()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    const string rawOldRefreshToken = "old-refresh-token";
    const string hashedOldRefreshToken = "hashed-old-refresh-token";

    const string rawNewRefreshToken = "new-refresh-token";
    const string hashedNewRefreshToken = "hashed-new-refresh-token";

    RefreshToken oldToken = RefreshToken.Create(
      user.Id,
      hashedOldRefreshToken,
      DateTimeOffset.UtcNow.AddMinutes(10));

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawOldRefreshToken);

    var tokenResult = new TokenResult(
      "new-access-token",
      DateTime.UtcNow.AddMinutes(15),
      "new-jti",
      "Bearer");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawOldRefreshToken))
      .Returns(hashedOldRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedOldRefreshToken))
      .ReturnsAsync(oldToken);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(user))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns(rawNewRefreshToken);

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawNewRefreshToken))
      .Returns(hashedNewRefreshToken);

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.Is<RefreshToken>(token =>
          token.UserId == user.Id &&
          token.Token == hashedNewRefreshToken),
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithValidToken_ShouldSaveChangesOnlyOnce()
  {
    // Arrange
    User user = new UserBuilder()
      .WithEmail("john@test.com")
      .Build();

    const string rawRefreshToken = "old-refresh-token";
    const string hashedRefreshToken = "hashed-old-refresh-token";

    RefreshToken oldToken = RefreshToken.Create(
      user.Id,
      hashedRefreshToken,
      DateTimeOffset.UtcNow.AddMinutes(10));

    var command = new RefreshTokenRotationCommand(
      "access-token",
      rawRefreshToken);

    var tokenResult = new TokenResult(
      "new-access-token",
      DateTime.UtcNow.AddMinutes(15),
      "new-jti",
      "Bearer");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken(rawRefreshToken))
      .Returns(hashedRefreshToken);

    _repositoryMock
      .Setup(x => x.RetrieveLastOldRefreshToken(hashedRefreshToken))
      .ReturnsAsync(oldToken);

    _repositoryMock
      .Setup(x => x.FindByIdAsync(user.Id, CancellationToken.None))
      .ReturnsAsync(user);

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(user))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns("new-refresh-token");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken("new-refresh-token"))
      .Returns("hashed-new-refresh-token");

    _uowMock
      .Setup(x => x.SaveChangesAsync(CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<RefreshTokenRotationResponse> result =
      await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    _uowMock.Verify(
      x => x.SaveChangesAsync(CancellationToken.None),
      Times.Once);
  }
}
