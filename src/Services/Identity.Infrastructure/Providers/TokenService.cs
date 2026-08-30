using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Options;
using Identity.Application.Abstractions.Auth;
using Identity.Application.Abstractions.Options;
using Identity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Identity.Infrastructure.Providers;

public sealed class TokenService : ITokenService
{
  private readonly JwtOptions _jwtOptions;

  private const string TokenType = "Bearer";

  public TokenService(IOptions<JwtOptions> options)
  {
    _jwtOptions = options.Value;
  }

  public string GenerateRefreshToken()
  {
    byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
    return Convert.ToBase64String(randomBytes);
  }

  public string HashRefreshToken(string rawToken)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(rawToken);
    byte[] hashed = SHA256.HashData(bytes);

    return Convert.ToHexString(hashed).ToLowerInvariant();
  }

  public TokenResult GenerateAccessToken(User user)
  {
    SigningCredentials credentials = _buildSigningCredentials();

    JwtAttributes attributes =
      _getJwtAttributes();

    List<Claim> claims = _getTokenClaims(user, attributes);

    SecurityTokenDescriptor tokenDescriptor = _buildSecurityTokenDescriptor(claims, attributes, credentials);

    var handler = new JwtSecurityTokenHandler();

    SecurityToken token = handler.CreateToken(tokenDescriptor);

    return new TokenResult(
      AccessToken: handler.WriteToken(token),
      ExpiresAt: attributes.ExpiresAt,
      Jti: attributes.Jti,
      TokenType: TokenType);
  }

  private JwtAttributes _getJwtAttributes()
  {
    DateTime now = DateTime.UtcNow;

    DateTime expiry = now.AddMinutes(_jwtOptions.ExpirationMinutes);

    string jti = Guid.NewGuid().ToString();
    string iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    return new JwtAttributes
    (
      Jti: jti,
      Iat: iat,
      NotBefore: now,
      IssuedAt: now,
      ExpiresAt: expiry,
      Issuer: _jwtOptions.Issuer,
      Audience: _jwtOptions.Audience
    );
  }

  private static List<Claim> _getTokenClaims(User user, JwtAttributes attributes)
  {
    return
    [
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, user.Email.Address),
      new(JwtRegisteredClaimNames.Jti, attributes.Jti),
      new(JwtRegisteredClaimNames.Iat, attributes.Iat, ClaimValueTypes.Integer64),
      new(ClaimTypes.Role, user.Role.Name)
    ];
  }

  private SecurityTokenDescriptor _buildSecurityTokenDescriptor(
    List<Claim> claims,
    JwtAttributes attributes,
    SigningCredentials credentials)
  {
    return new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      NotBefore = attributes.NotBefore,
      IssuedAt = attributes.IssuedAt,
      Expires = attributes.ExpiresAt,
      Issuer = _jwtOptions.Issuer,
      Audience = _jwtOptions.Audience,
      SigningCredentials = credentials
    };
  }

  private SigningCredentials _buildSigningCredentials()
  {

    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
  }
}
