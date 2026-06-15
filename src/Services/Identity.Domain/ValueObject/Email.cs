using System.Text.RegularExpressions;
using BuildingBlocks.Exceptions;

namespace Identity.Domain.ValueObject;

public sealed class Email : BuildingBlocks.Abstractions.ValueObject
{
  public string Address { get; }

  private Email()
  {
    Address = null!;
  }

  private Email(string address)
  {
    Address = address;
  }

  public static Email Create(string emailAddress)
  {
    string emailRegexPattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

    if (string.IsNullOrWhiteSpace(emailAddress))
    {
      throw new DomainException("Email Address cannot be null or whitespaced");
    }

    if (emailAddress.Length <= 5)
    {
      throw new DomainException("Email Address must have more than 5 characters");
    }

    if (!Regex.IsMatch(emailAddress, emailRegexPattern))
    {
      throw new DomainException("Email Address needs to be a valid email");
    }

    return new Email(emailAddress.ToLower().Trim());
  }

  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return Address;
  }
}
