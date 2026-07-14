namespace Identity.Application.Abstractions.Auth;

public interface IExternalIdentityProvider
{
  string ProviderName { get; }

  Task<ExternalUserInfo?> ValidateTokenAsync(string token, CancellationToken ct = default);
}

public sealed record ExternalUserInfo(
  string ProviderId,
  string Email,
  string? FirstName,
  string? LastName,
  string Provider);
