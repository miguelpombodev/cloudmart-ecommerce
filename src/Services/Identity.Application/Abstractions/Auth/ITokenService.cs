using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Auth;

public interface ITokenService
{
  TokenResult GenerateAccessToken(User user);
  string GenerateRefreshToken();
  string HashRefreshToken(string rawToken);
}

public sealed record TokenResult(
  string AccessToken,
  DateTime ExpiresAt,
  string Jti,
  string TokenType
);

public sealed record JwtAttributes(
  DateTime ExpiresAt,
  string Jti,
  string Iat,
  DateTime NotBefore,
  DateTime IssuedAt,
  string Issuer,
  string Audience
);
