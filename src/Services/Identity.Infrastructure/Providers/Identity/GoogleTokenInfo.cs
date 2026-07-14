using System.Text.Json.Serialization;

namespace Identity.Infrastructure.Providers.Identity;

internal sealed record GoogleTokenInfo(
  [property: JsonPropertyName("sub")] string Sub,
  [property: JsonPropertyName("email")] string Email,
  [property: JsonPropertyName("given_name")]
  string? GivenName,
  [property: JsonPropertyName("family_name")]
  string? FamilyName,
  [property: JsonPropertyName("aud")] string Audience,
  [property: JsonPropertyName("email_verified")]
  string EmailVerified);
