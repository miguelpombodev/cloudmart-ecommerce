using FluentAssertions;
using Identity.Application.Features.RefreshTokens.Rotation;

namespace Identity.Tests.Application.RefreshTokens.RefreshTokenRotation;

public class RefreshTokenRotationValidatorTests
{
  private readonly RefreshTokenRotationValidator _validator;

  public RefreshTokenRotationValidatorTests()
  {
    _validator = new RefreshTokenRotationValidator();
  }

  [Fact]
  public void Validate_WithValidRefreshToken_ShouldBeValid()
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: "1234567890");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_WithEmptyRefreshToken_ShouldBeInvalid()
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: string.Empty);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().ContainSingle(error =>
      error.PropertyName == nameof(RefreshTokenRotationCommand.OldRefreshToken) &&
      error.ErrorMessage == "Expired refresh token is required");
  }

  [Fact]
  public void Validate_WithNullRefreshToken_ShouldBeInvalid()
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: null!);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().Contain(error =>
      error.PropertyName == nameof(RefreshTokenRotationCommand.OldRefreshToken) &&
      error.ErrorMessage == "Expired refresh token is required");
  }

  [Fact]
  public void Validate_WithRefreshTokenShorterThan10Characters_ShouldBeInvalid()
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: "123456789");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().ContainSingle(error =>
      error.PropertyName == nameof(RefreshTokenRotationCommand.OldRefreshToken) &&
      error.ErrorMessage == "Invalid refresh token format");
  }

  [Fact]
  public void Validate_WithRefreshTokenExactly10Characters_ShouldBeValid()
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: "1234567890");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
  }

  [Theory]
  [InlineData("1")]
  [InlineData("12345")]
  [InlineData("123456789")]
  public void Validate_WithRefreshTokenShorterThan10Characters_ShouldReturnFormatError(
    string refreshToken)
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: refreshToken);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().ContainSingle(error =>
      error.PropertyName == nameof(RefreshTokenRotationCommand.OldRefreshToken) &&
      error.ErrorMessage == "Invalid refresh token format");
  }

  [Theory]
  [InlineData("1234567890")]
  [InlineData("12345678901")]
  [InlineData("abcdefghijklmnopqrstuvwxyz")]
  [InlineData("********************************")]
  public void Validate_WithRefreshTokenOfAtLeast10Characters_ShouldBeValid(
    string refreshToken)
  {
    // Arrange
    var command = new RefreshTokenRotationCommand(
      AccessToken: "valid-access-token",
      OldRefreshToken: refreshToken);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
  }
}

