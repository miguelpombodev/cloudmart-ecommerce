using BuildingBlocks.Exceptions;
using FluentAssertions;
using Identity.Domain.ValueObject;

namespace Identity.Tests.Domain.ValueObjects;

public class PasswordTests
{
  [Fact]
  public void Create_WithValidStringPassword_ShouldSucceed()
  {
    // Act
    Func<Password> act = () => Password.CreateWithNoProvider("Senha@123");

    // Assert
    act.Should().NotThrow();
  }

  [Theory]
  [InlineData("short1!")]
  [InlineData("")]
  [InlineData("   ")]
  public void Create_WithTooShortPassword_ShouldThrowDomainException(string shortPassword)
  {
    // Act
    Func<Password> act = () => Password.CreateWithNoProvider(shortPassword);

    // Assert
    act.Should().Throw<DomainException>().WithMessage("Password must have at least 8 characters");
  }

  [Theory]
  [InlineData("alllowercase1!")]
  [InlineData("ALLUPPERCASE1!")]
  [InlineData("NoNumbersHere!")]
  [InlineData("NoSpecialChar123")]
  [InlineData("Has Space123!")]
  public void Create_WithPasswordMissingComplexityRule_ShouldThrowDomainException(string weakPassword)
  {
    // Act
    Func<Password> act = () => Password.CreateWithNoProvider(weakPassword);

    // Assert
    act.Should().Throw<DomainException>()
      .WithMessage(
        "Password must respect the defined rules, *");
  }

  [Fact]
  public void Create_ShouldNeverExposePlainPassword()
  {
    // Arrange
    const string plainPassword = "Senha@123";
    var password = Password.CreateWithNoProvider(plainPassword);

    // Assert

    // Bcrypt produces hashes that always start with an algorithm version identifier (e.g. $2a$, $2b$).
    // Assert this prefix is a confirmation that Bcrypt in fact hashed this one, and not other process.
    password.HashedValue.Should().NotBe(plainPassword);
    password.HashedValue.Should().StartWith("$2");
  }

  [Fact]
  public void Verify_WithCorrectPlainPassword_ShouldReturnTrue()
  {
    // Arrange
    const string plainPassword = "Senha@123";
    var password = Password.CreateWithNoProvider(plainPassword);

    // Act
    bool isValid = password.Verify(plainPassword);

    isValid.Should().BeTrue();
  }

  [Fact]
  public void Verify_WithIncorrectPlainPassword_ShouldReturnFalse()
  {
    var password = Password.CreateWithNoProvider("Senha@123");

    bool isValid = password.Verify("WrongPassword@456");

    isValid.Should().BeFalse();
  }

  [Fact]
  public void Create_SamePlainPasswordTwice_ShouldProduceDifferentHashes()
  {
    const string plainPassword = "Senha@123";

    var passwordA = Password.CreateWithNoProvider(plainPassword);
    var passwordB = Password.CreateWithNoProvider(plainPassword);

    passwordA.HashedValue.Should().NotBe(passwordB.HashedValue);

    passwordA.Verify(plainPassword).Should().BeTrue();
    passwordB.Verify(plainPassword).Should().BeTrue();
  }

  [Fact]
  public void Equals_TwoPasswordWithDifferentSaltButSamePlainText_ShouldNotBeEqual()
  {
    const string plainPassword = "Senha@123";

    var passwordA = Password.CreateWithNoProvider(plainPassword);
    var passwordB = Password.CreateWithNoProvider(plainPassword);

    passwordA.Should().NotBe(passwordB);
  }
}
