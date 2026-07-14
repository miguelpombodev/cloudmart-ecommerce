using System.Text.Json.Serialization;

namespace Cloudmart.Identity.Features.Users.Login;

/// <summary>
///   Request DTO to handle Google Social Login
/// </summary>
/// <param name="idToken"></param>
public sealed record GoogleLoginRequest(
  [property: JsonPropertyName("id_token")]
  string idToken);
