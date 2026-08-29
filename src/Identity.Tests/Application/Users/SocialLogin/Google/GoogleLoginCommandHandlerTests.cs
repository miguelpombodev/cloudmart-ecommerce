using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure;
using FluentAssertions;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Repositories;
using Identity.Application.Features.Users.Login;
using Identity.Application.Features.Users.SocialLogin;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObject;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Tests.Application.Users.SocialLogin.Google;

public class GoogleLoginCommandHandlerTests
{
  private readonly GoogleLoginCommandHandler _handler;

  private readonly Mock<IUserRepository> _repositoryMock;

  private readonly Mock<IRoleRepository> _roleRepositoryMock;

  private readonly Mock<ITokenService> _tokenServiceMock;

  private readonly Mock<IExternalIdentityProvider> _googleProviderMock;

  private readonly Mock<IUnitOfWork> _uowMock;

  public GoogleLoginCommandHandlerTests()
  {
    _repositoryMock = new Mock<IUserRepository>();
    _roleRepositoryMock = new Mock<IRoleRepository>();
    _tokenServiceMock = new Mock<ITokenService>();
    _googleProviderMock = new Mock<IExternalIdentityProvider>();
    _uowMock = new Mock<IUnitOfWork>();

    var logger = new Mock<ILogger<GoogleLoginCommandHandler>>();

    _handler = new GoogleLoginCommandHandler(
      _repositoryMock.Object,
      _roleRepositoryMock.Object,
      _tokenServiceMock.Object,
      _googleProviderMock.Object,
      logger.Object,
      _uowMock.Object);
  }

  [Fact]
  public async Task Handle_WhenGoogleTokenIsInvalid_ShouldReturnUnauthorized()
  {
    // Arrange
    var command = new GoogleLoginCommand("invalid-google-token");

    _googleProviderMock
      .Setup(x => x.ValidateTokenAsync(
        command.IdToken,
        CancellationToken.None))
      .ReturnsAsync((ExternalUserInfo?)null);

    // Act
    Result<LoginResponse> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.StatusCode.Should().Be(401);
    result.Error.Description.Should()
      .Be("Invalid or expired Google token");

    _repositoryMock.Verify(
      x => x.FindByEmail(It.IsAny<string>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenGoogleAccountHasNoEmail_ShouldReturnUnauthorized()
  {
    // Arrange
    var command = new GoogleLoginCommand("valid-google-token");

    var externalUser = new ExternalUserInfo(
      Email: string.Empty,
      FirstName: "John",
      LastName: "Doe",
      Provider: "Google",
      ProviderId: "google-123");

    _googleProviderMock
      .Setup(x => x.ValidateTokenAsync(
        command.IdToken,
        CancellationToken.None))
      .ReturnsAsync(externalUser);

    // Act
    Result<LoginResponse> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.StatusCode.Should().Be(401);
    result.Error.Description.Should()
      .Be("Google account does not have a verified email");

    _repositoryMock.Verify(
      x => x.FindByEmail(It.IsAny<string>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task Handle_WhenUserDoesNotExist_ShouldCreateUserAndLogin()
  {
    // Arrange
    var command = new GoogleLoginCommand("valid-google-token");

    var externalUser = new ExternalUserInfo(
      Email: "john.doe@gmail.com",
      FirstName: "John",
      LastName: "Doe",
      Provider: "Google",
      ProviderId: "google-123");

    Role customerRole = Role.Create(nameof(RoleType.Customer));

    var tokenResult = new TokenResult(
      "jwt-token",
      DateTime.UtcNow.AddMinutes(15),
      "test-jti",
      "Bearer");

    _googleProviderMock
      .Setup(x => x.ValidateTokenAsync(
        command.IdToken,
        CancellationToken.None))
      .ReturnsAsync(externalUser);

    _repositoryMock
      .Setup(x => x.FindByEmail(externalUser.Email))
      .ReturnsAsync((User?)null);

    _roleRepositoryMock
      .Setup(x => x.FindRoleByName(
        nameof(RoleType.Customer),
        CancellationToken.None))
      .ReturnsAsync(customerRole);

    _repositoryMock
      .Setup(x => x.AddAsync(
        It.IsAny<User>(),
        CancellationToken.None));

    _repositoryMock
      .Setup(x => x.FindUserAuthenticationByProviderAsync(
        externalUser.Email,
        externalUser.Provider,
        CancellationToken.None))
      .ReturnsAsync((UserAuthenticationProvider?)null);

    _repositoryMock
      .Setup(x => x.AddUserAuthenticationProviderAsync(
        It.IsAny<UserAuthenticationProvider>(),
        CancellationToken.None));

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns("raw-refresh-token");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken("raw-refresh-token"))
      .Returns("hashed-refresh-token");

    _repositoryMock
      .Setup(x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(
        CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<LoginResponse> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.AccessToken.Should().Be("jwt-token");
    result.Value.RefreshToken.Should().Be("raw-refresh-token");
    result.Value.TokenType.Should().Be("Bearer");

    result.Value.ExpiresAt.Should()
      .BeOnOrAfter(DateTimeOffset.UtcNow.AddMinutes(1).AddSeconds(59));

    result.Value.ExpiresAt.Should()
      .BeOnOrBefore(DateTimeOffset.UtcNow.AddMinutes(2).AddSeconds(1));

    _repositoryMock.Verify(
      x => x.AddAsync(
        It.Is<User>(user =>
          user.Email.Address == externalUser.Email &&
          user.Name.ToString().Contains("John")),
        CancellationToken.None),
      Times.Once);

    _roleRepositoryMock.Verify(
      x => x.FindRoleByName(
        nameof(RoleType.Customer),
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.AddUserAuthenticationProviderAsync(
        It.Is<UserAuthenticationProvider>(provider =>
          provider.UserId != Guid.Empty),
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserExistsAndIsActive_ShouldLoginSuccessfully()
  {
    // Arrange
    User user = CreateUser();

    var command = new GoogleLoginCommand("valid-google-token");

    var externalUser = new ExternalUserInfo(
      Email: user.Email.Address,
      FirstName: "John",
      LastName: "Doe",
      Provider: "Google",
      ProviderId: "google-123");

    var existingAuthenticationProvider =
      UserAuthenticationProvider.Create(
        user.Id,
        externalUser.Provider,
        externalUser.ProviderId,
        user.Email);

    var tokenResult = new TokenResult(
      "jwt-token",
      DateTime.UtcNow.AddMinutes(15),
      "test-jti",
      "Bearer");

    _googleProviderMock
      .Setup(x => x.ValidateTokenAsync(
        command.IdToken,
        CancellationToken.None))
      .ReturnsAsync(externalUser);

    _repositoryMock
      .Setup(x => x.FindByEmail(externalUser.Email))
      .ReturnsAsync(user);

    _repositoryMock
      .Setup(x => x.FindUserAuthenticationByProviderAsync(
        externalUser.Email,
        externalUser.Provider,
        CancellationToken.None))
      .ReturnsAsync(existingAuthenticationProvider);

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(user))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns("raw-refresh-token");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken("raw-refresh-token"))
      .Returns("hashed-refresh-token");

    _repositoryMock
      .Setup(x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(
        CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<LoginResponse> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    result.Value.AccessToken.Should().Be("jwt-token");
    result.Value.RefreshToken.Should().Be("raw-refresh-token");
    result.Value.TokenType.Should().Be("Bearer");

    _repositoryMock.Verify(
      x => x.AddUserAuthenticationProviderAsync(
        It.IsAny<UserAuthenticationProvider>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserExistsWithoutAuthenticationProvider_ShouldCreateProvider()
  {
    // Arrange
    User user = CreateUser();

    var command = new GoogleLoginCommand("valid-google-token");

    var externalUser = new ExternalUserInfo(
      Email: user.Email.Address,
      FirstName: "John",
      LastName: "Doe",
      Provider: "Google",
      ProviderId: "google-123");

    var tokenResult = new TokenResult(
      "jwt-token",
      DateTime.UtcNow.AddMinutes(15),
      "test-jti",
      "Bearer");

    _googleProviderMock
      .Setup(x => x.ValidateTokenAsync(
        command.IdToken,
        CancellationToken.None))
      .ReturnsAsync(externalUser);

    _repositoryMock
      .Setup(x => x.FindByEmail(externalUser.Email))
      .ReturnsAsync(user);

    _repositoryMock
      .Setup(x => x.FindUserAuthenticationByProviderAsync(
        externalUser.Email,
        externalUser.Provider,
        CancellationToken.None))
      .ReturnsAsync((UserAuthenticationProvider?)null);

    _repositoryMock
      .Setup(x => x.AddUserAuthenticationProviderAsync(
        It.IsAny<UserAuthenticationProvider>(),
        CancellationToken.None));

    _tokenServiceMock
      .Setup(x => x.GenerateAccessToken(user))
      .Returns(tokenResult);

    _tokenServiceMock
      .Setup(x => x.GenerateRefreshToken())
      .Returns("raw-refresh-token");

    _tokenServiceMock
      .Setup(x => x.HashRefreshToken("raw-refresh-token"))
      .Returns("hashed-refresh-token");

    _repositoryMock
      .Setup(x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None));

    _uowMock
      .Setup(x => x.SaveChangesAsync(
        CancellationToken.None))
      .ReturnsAsync(1);

    // Act
    Result<LoginResponse> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();

    _repositoryMock.Verify(
      x => x.AddUserAuthenticationProviderAsync(
        It.Is<UserAuthenticationProvider>(provider =>
          provider.UserId == user.Id),
        CancellationToken.None),
      Times.Once);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        CancellationToken.None),
      Times.Once);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        CancellationToken.None),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenUserIsInactive_ShouldReturnUnauthorized()
  {
    // Arrange
    User user = CreateUser();

    user.InactivateUser();

    var command = new GoogleLoginCommand("valid-google-token");

    var externalUser = new ExternalUserInfo(
      Email: user.Email.Address,
      FirstName: "John",
      LastName: "Doe",
      Provider: "Google",
      ProviderId: "google-123");

    _googleProviderMock
      .Setup(x => x.ValidateTokenAsync(
        command.IdToken,
        CancellationToken.None))
      .ReturnsAsync(externalUser);

    _repositoryMock
      .Setup(x => x.FindByEmail(externalUser.Email))
      .ReturnsAsync(user);

    // Act
    Result<LoginResponse> result =
      await _handler.Handle(
        command,
        CancellationToken.None);

    // Assert
    result.IsFailure.Should().BeTrue();

    result.Error.StatusCode.Should().Be(401);
    result.Error.Description.Should()
      .Be("Invalid credentials");

    _repositoryMock.Verify(
      x => x.FindUserAuthenticationByProviderAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.AddUserAuthenticationProviderAsync(
        It.IsAny<UserAuthenticationProvider>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _repositoryMock.Verify(
      x => x.AddRefreshToken(
        It.IsAny<RefreshToken>(),
        It.IsAny<CancellationToken>()),
      Times.Never);

    _uowMock.Verify(
      x => x.SaveChangesAsync(
        It.IsAny<CancellationToken>()),
      Times.Never);
  }

  private static User CreateUser()
  {
    Role role = Role.Create(nameof(RoleType.Customer));

    return User.Create(
      CompleteName.Create("John", "Doe"),
      Email.Create("john@test.com"),
      Password.CreateWithProvider(null),
      role);
  }
}

