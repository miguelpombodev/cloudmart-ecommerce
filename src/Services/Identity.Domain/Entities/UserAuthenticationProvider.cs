using System.Text.RegularExpressions;
using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Identity.Domain.ValueObject;

namespace Identity.Domain.Entities;

public sealed class UserAuthenticationProvider : Entity<Guid>
{
  private UserAuthenticationProvider()
  {
    Provider = null!;
    ProviderUserId = null!;
    Email = null!;
  }

  private UserAuthenticationProvider(
    Guid userId,
    string provider,
    string providerUserId,
    Email email)
  {
    UserId = userId;
    Provider = provider;
    ProviderUserId = providerUserId;
    Email = email;
  }

  public Guid UserId { get; private set; }

  public string Provider { get; set; }

  public string ProviderUserId { get; set; }

  public Email Email { get; }

  public static UserAuthenticationProvider Create(Guid userId, string providerName, string providerUserId, Email email)
  {
    string providerNameStartedWithNumberRegexPattern = @"^\d";

    if (string.IsNullOrWhiteSpace(providerName) ||
        string.IsNullOrWhiteSpace(providerUserId) ||
        Regex.IsMatch(providerName, providerNameStartedWithNumberRegexPattern))
    {
      throw new DomainException("Providers info must be valid strings");
    }

    return new UserAuthenticationProvider(userId, providerName, providerUserId, email)
    {
      Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
    };
  }
}
