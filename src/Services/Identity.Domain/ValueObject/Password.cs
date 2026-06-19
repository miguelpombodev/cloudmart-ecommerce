using System.Text.RegularExpressions;
using BuildingBlocks.Exceptions;

namespace Identity.Domain.ValueObject;

public sealed class Password : BuildingBlocks.Abstractions.ValueObject
{
  public string HashedValue { get; }

  private Password()
  {
    HashedValue = null!;
  }

  private Password(string hashedPassword)
  {
    HashedValue = hashedPassword;
  }

  public static Password Create(string plainPassword)
  {
    string regexPattern = @"^(?!.*\s)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

    if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 8)
    {
      throw new DomainException("Password must have at least 8 characters");
    }

    if (!Regex.IsMatch(plainPassword, regexPattern))
    {
      throw new DomainException(
        "Password must respect the defined rules, such as, at least one character lowercase, one character uppercase, one numeric digit and one special digit");
    }

    return new Password(Hash(plainPassword));
  }

  public bool Verify(string plainPassword) =>
    BCrypt.Net.BCrypt.Verify(plainPassword, HashedValue);

  protected override IEnumerable<object?> RetrieveEqualityComponents()
  {
    yield return HashedValue;
  }

  private static string Hash(string plainPassword) =>
    BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
}
