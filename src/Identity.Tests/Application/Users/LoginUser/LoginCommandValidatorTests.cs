using FluentAssertions;
using Identity.Application.Features.Users.Login;

namespace Identity.Tests.Application.Users.Login;

public class LoginCommandValidatorTests
{
  private readonly LoginCommandValidator _validator;

  public LoginCommandValidatorTests()
  {
    _validator = new LoginCommandValidator();
  }

  [Fact]
  public void Validate_WithValidCredentials_ShouldBeValid()
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      "Password@123!");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void Validate_WithEmptyEmail_ShouldBeInvalid()
  {
    // Arrange
    var command = new LoginCommand(
      string.Empty,
      "Password@123!");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().Contain(error =>
      error.PropertyName == nameof(LoginCommand.Email) &&
      error.ErrorMessage == "Email is required");
  }

  [Fact]
  public void Validate_WithInvalidEmail_ShouldBeInvalid()
  {
    // Arrange
    var command = new LoginCommand(
      "invalid-email",
      "Password@123!");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().Contain(error =>
      error.PropertyName == nameof(LoginCommand.Email) &&
      error.ErrorMessage == "Email address is not valid");
  }

  [Theory]
  [InlineData("john")]
  [InlineData("john@")]
  [InlineData("@test.com")]
  [InlineData("john@test")]
  [InlineData("john.test.com")]
  public void Validate_WithInvalidEmailFormat_ShouldReturnEmailFormatError(
    string email)
  {
    // Arrange
    var command = new LoginCommand(
      email,
      "Password@123!");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().Contain(error =>
      error.PropertyName == nameof(LoginCommand.Email) &&
      error.ErrorMessage == "Email address is not valid");
  }

  [Theory]
  [InlineData("john@test.com")]
  [InlineData("john.doe@test.com")]
  [InlineData("john+login@test.com")]
  [InlineData("john_doe@test.com")]
  public void Validate_WithValidEmail_ShouldNotHaveEmailErrors(
    string email)
  {
    // Arrange
    var command = new LoginCommand(
      email,
      "Password@123!");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.Errors
      .Where(error => error.PropertyName == nameof(LoginCommand.Email))
      .Should()
      .BeEmpty();
  }

  [Fact]
  public void Validate_WithPasswordShorterThan8Characters_ShouldBeInvalid()
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      "1234567");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().ContainSingle(error =>
      error.PropertyName == nameof(LoginCommand.Password) &&
      error.ErrorMessage == "Password must have at least 8 characters");
  }

  [Fact]
  public void Validate_WithPasswordExactly8Characters_ShouldBeValid()
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      "12345678");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
  }

  [Theory]
  [InlineData("1234567")]
  [InlineData("short")]
  [InlineData("123")]
  public void Validate_WithPasswordShorterThan8Characters_ShouldReturnPasswordError(
    string password)
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      password);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.Errors.Should().ContainSingle(error =>
      error.PropertyName == nameof(LoginCommand.Password) &&
      error.ErrorMessage == "Password must have at least 8 characters");
  }

  [Theory]
  [InlineData("12345678")]
  [InlineData("Password")]
  [InlineData("Password@123!")]
  [InlineData("abcdefghijk")]
  public void Validate_WithPasswordOfAtLeast8Characters_ShouldNotHavePasswordErrors(
    string password)
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      password);

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.Errors
      .Where(error => error.PropertyName == nameof(LoginCommand.Password))
      .Should()
      .BeEmpty();
  }

  [Fact]
  public void Validate_WithValidEmailAndShortPassword_ShouldOnlyReturnPasswordError()
  {
    // Arrange
    var command = new LoginCommand(
      "john@test.com",
      "1234567");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().ContainSingle();

    result.Errors.Should().Contain(error =>
      error.PropertyName == nameof(LoginCommand.Password) &&
      error.ErrorMessage == "Password must have at least 8 characters");
  }

  [Fact]
  public void Validate_WithInvalidEmailAndValidPassword_ShouldOnlyReturnEmailError()
  {
    // Arrange
    var command = new LoginCommand(
      "invalid-email",
      "Password@123!");

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should().ContainSingle();

    result.Errors.Should().Contain(error =>
      error.PropertyName == nameof(LoginCommand.Email) &&
      error.ErrorMessage == "Email address is not valid");
  }
}

