using BuildingBlocks.Exceptions;
using FluentAssertions;
using Identity.Domain.ValueObject;

namespace Identity.Tests.Domain.ValueObjects;

public class EmailTests
{
  [Theory]
  [InlineData("user@example.com")]
  [InlineData("first.last@example.com")]
  [InlineData("user+tag@example.co.uk")]
  [InlineData("user_name@example-domain.com")]
  public void Create_WithValidEmailFormat_ShouldSucceed(string validEmail)
  {
    // Arrange and Act
    var email = Email.Create(validEmail);

    // Assert
    email.Should().NotBeNull();
    email.Address.Should().Be(validEmail.ToLower().Trim());
  }

  [Fact]
  public void Create_WithUppercaseEmail_ShouldNormalizeToLowercase()
  {
    // Arrange
    const string emailWithUppercase = "User@Example.COM";

    // Act
    var email = Email.Create(emailWithUppercase);

    // Assert
    email.Address.Should().Be("user@example.com");
  }

  [Fact]
  public void Create_WithLeadingOrTrailingWhitespace_ShouldTrim()
  {
    // Arrange
    const string emailWithSpaces = "    user@example.com   ";

    // Act
    var email = Email.Create(emailWithSpaces);

    // Assert
    email.Address.Should().Be("user@example.com");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void Create_WithNullOrWhitespaces_ShouldThrowDomainException(string? invalidEmail)
  {
    // Act
    Func<Email> act = () => Email.Create(invalidEmail!);

    // Assert
    // For FluentAssertion, the "Action" syntax in lambda allows to verify exceptions fluently
    // including the message
    act.Should().Throw<DomainException>().WithMessage("Email Address cannot be null or whitespaced");
  }

  [Theory]
  [InlineData("a@b.c")]
  [InlineData("a@bc")]
  public void Create_WithTooShortEmail_ShouldThrowDomainException(string shortEmail)
  {
    // Act
    Func<Email> act = () => Email.Create(shortEmail);

    // Assert
    act.Should().Throw<DomainException>().WithMessage("Email Address must have more than 5 characters");
  }

  [Theory]
  [InlineData("notaemail")]
  [InlineData("missing@domain")]
  [InlineData("@example.com")]
  [InlineData("user@@example.com")]
  [InlineData("user@.com")]
  public void Create_WithInvalidFormat_ShouldThrowDomainException(string invalidEmail)
  {
    // Act
    Func<Email> act = () => Email.Create(invalidEmail);

    // Assert
    act.Should().Throw<DomainException>().WithMessage("Email Address needs to be a valid email");
  }

  [Fact]
  public void Equals_TwoEmailsWithSameAddress_ShouldBeEqual()
  {
    // Arrange
    var emailA = Email.Create("user@example.com");
    var emailB = Email.Create("USER@EXAMPLE.COM");

    // Assert
    emailA.Should().Be(emailB);
    (emailA == emailB).Should().BeTrue();
  }

  [Fact]
  public void Equals_TwoEmailWithDifferentAddress_ShouldNotBeEqual()
  {
    // Arrange
    var emailA = Email.Create("userA@example.com");
    var emailB = Email.Create("userB@example.com");

    emailA.Should().NotBe(emailB);
    (emailA != emailB).Should().BeTrue();
  }

  [Fact]
  public void GetHashCode_TwoEqualEmail_ShouldHaveSameHashCode()
  {
    var emailA = Email.Create("user@example.com");
    var emailB = Email.Create("user@example.com");

    emailA.GetHashCode().Should().Be(emailB.GetHashCode());
  }

  [Fact]
  public void HashSet_WithDuplicateEmails_ShouldDeduplicateBasedOnValue()
  {
    var emails = new HashSet<Email>
    {
      Email.Create("user@example.com"), Email.Create("USER@EXAMPLE.COM"), Email.Create("other@example.com")
    };

    emails.Should().HaveCount(2);
  }
}
