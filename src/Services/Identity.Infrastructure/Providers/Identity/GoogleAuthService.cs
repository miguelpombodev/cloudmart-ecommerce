using System.Text.Json;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Providers.Identity;

public sealed class GoogleAuthService : IExternalIdentityProvider
{
  // Google's endpoint that validates id_token and return claims
  private const string TokenInfoUrl = "tokeninfo?id_token=";

  private readonly ExternalProvidersOptions _externalProvidersOptions;

  private readonly HttpClient _httpClient;

  private readonly ILogger<GoogleAuthService> _logger;

  public GoogleAuthService(
    HttpClient httpClient,
    IOptions<ExternalProvidersOptions> externalProvidersOptions,
    ILogger<GoogleAuthService> logger
  )
  {
    _httpClient = httpClient;
    _externalProvidersOptions = externalProvidersOptions.Value;
    _logger = logger;
  }

  public string ProviderName => "google";

  public async Task<ExternalUserInfo?> ValidateTokenAsync(string token, CancellationToken ct = default)
  {
    HttpResponseMessage response = await _httpClient.GetAsync(
      $"{TokenInfoUrl}{token}", ct);

    if (!response.IsSuccessStatusCode)
    {
      _logger.LogWarning(
        "Google token validation failed with status {Status}",
        response.StatusCode);

      return null;
    }

    string content = await response.Content.ReadAsStringAsync(ct);
    GoogleTokenInfo? claims = JsonSerializer.Deserialize<GoogleTokenInfo>(content);

    if (claims is null)
    {
      return null;
    }

    if (claims.Audience == _externalProvidersOptions.Google.ClientId)
    {
      return new ExternalUserInfo(
        claims.Sub,
        claims.Email,
        claims.GivenName,
        claims.FamilyName,
        ProviderName.ToUpperInvariant());
    }

    _logger.LogWarning(
      "Google token audience mismatch. Expected {Expected}, got {Actual}",
      _externalProvidersOptions.Google.ClientId, claims.Audience);

    return null;
  }
}
