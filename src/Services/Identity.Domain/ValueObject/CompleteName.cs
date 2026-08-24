using System.Text.RegularExpressions;
using BuildingBlocks.Exceptions;

namespace Identity.Domain.ValueObject;

public class CompleteName : BuildingBlocks.Abstractions.ValueObject
{
  private CompleteName()
  {
    FirstName = null!;
    LastName = null!;
  }

  private CompleteName(string firstName, string lastName)
  {
    FirstName = firstName;
    LastName = lastName;
  }

  public string FirstName { get; }

  public string LastName { get; }

  public string Initials => $"{char.ToUpper(FirstName[0])}{char.ToUpper(LastName[0])}";

  public static CompleteName Create(string firstName, string lastName)
  {
    string nameStartedWithNumberRegexPattern = @"^\d";

    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
    {
      throw new DomainException("First and Last Names must have any value");
    }

    if (Regex.IsMatch(firstName, nameStartedWithNumberRegexPattern) ||
        Regex.IsMatch(lastName, nameStartedWithNumberRegexPattern))
    {
      throw new DomainException("First and Last Names must not start with any number");
    }

    return new CompleteName(firstName, lastName);
  }

  public override string ToString() =>
   $"{FirstName} {LastName}";

  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return FirstName.ToLower();
    yield return LastName.ToLower();
  }

}
