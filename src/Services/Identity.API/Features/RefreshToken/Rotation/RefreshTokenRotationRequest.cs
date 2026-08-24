namespace Cloudmart.Identity.Features.RefreshToken.Rotation;

/// <summary>
///   Request DTO to rotate user's refresh token
/// </summary>
/// <param name="oldRefreshToken"></param>
public record RefreshTokenRotationRequest(string oldRefreshToken);
