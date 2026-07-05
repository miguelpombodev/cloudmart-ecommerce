namespace Identity.Domain.Entities;

public class RefreshToken
{
  private RefreshToken()
  {
    Token = null!;
  }

  private RefreshToken(
    Guid id,
    Guid userId,
    string hashedToken,
    DateTimeOffset expiresAt,
    DateTimeOffset createdAt,
    bool isRevoked,
    DateTimeOffset? revokedAt)
  {
    Id = id;
    UserId = userId;
    Token = hashedToken;
    ExpiresAt = expiresAt;
    CreatedAt = createdAt;
    IsRevoked = isRevoked;
    RevokedAt = revokedAt;
  }

  public Guid Id { get; private set; }

  public Guid UserId { get; private set; }

  public DateTimeOffset ExpiresAt { get; }

  public DateTimeOffset CreatedAt { get; private set; }

  public bool IsRevoked { get; private set; }

  public DateTimeOffset? RevokedAt { get; private set; }

  public string Token { get; private set; }

  public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

  public bool IsActive => !IsRevoked && !IsExpired;

  public static RefreshToken Create(Guid userId, string hashedToken, DateTimeOffset expiresAt) =>
    new(Guid.NewGuid(), userId, hashedToken, expiresAt, DateTimeOffset.UtcNow, false, null);

  public void RevokeToken()
  {
    IsRevoked = true;
    RevokedAt = DateTime.UtcNow;
  }
}
