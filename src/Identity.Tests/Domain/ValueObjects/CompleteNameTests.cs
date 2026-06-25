using BuildingBlocks.Exceptions;
using FluentAssertions;
using Identity.Domain.ValueObject;

namespace Identity.Tests.Domain.ValueObjects;

public class CompleteNameTests
{
  [Fact]
  public void Create_WithValidNames_ShouldSucceed()
  {
    string firstName = "João";
    string lastName = "Silva";

    var name = CompleteName.Create(firstName, lastName);

    name.FirstName.Should().Be(firstName);
    name.LastName.Should().Be(lastName);
  }

  [Theory]
  [InlineData("João", "Silva", "JS")]
  [InlineData("ana", "costa", "AC")]
  [InlineData("Maria", "DOS SANTOS", "MD")]
  public void Initials_ShouldBeComputedFromFirstAndLastName(
    string firstName,
    string lastName,
    string expectedInitials)
  {
    var name = CompleteName.Create(firstName, lastName);

    name.Initials.Should().Be(expectedInitials);
  }

  [Theory]
  [InlineData(null, "Silva")]
  [InlineData("João", null)]
  [InlineData("", "Silva")]
  [InlineData("João", "")]
  [InlineData("  ", "Silva")]
  public void Create_WithNullOrWhitespaceName_ShouldThrowDomainException(
    string? firstName,
    string? lastName
  )
  {
    // Act
    Func<CompleteName> act = () => CompleteName.Create(firstName!, lastName!);

    // Assert
    act.Should().Throw<DomainException>().WithMessage("First and Last Names must have any value");
  }

  [Theory]
  [InlineData("1 João", "Silva")]
  [InlineData("João", "2Silva")]
  [InlineData("99", "00")]
  public void Create_WithNameStartingWithNumber_ShouldThrowDomainException(
    string firstName,
    string lastName)
  {
    Func<CompleteName> act = () => CompleteName.Create(firstName, lastName);

    act.Should()
      .Throw<DomainException>()
      .WithMessage("First and Last Names must not start with any number");
  }

  [Fact]
  public void Equals_NamesWithDifferentCasing_ShouldBeEqual()
  {
    var nameA = CompleteName.Create("João", "Silva");
    var nameB = CompleteName.Create("JOÃO", "SILVA");

    nameA.Should().Be(nameB);
  }

  [Fact]
  public void Equals_DifferentNames_ShouldNotBeEqual()
  {
    var nameA = CompleteName.Create("João", "Silva");
    var nameB = CompleteName.Create("Pedro", "Santos");

    nameA.Should().NotBe(nameB);
  }
}
