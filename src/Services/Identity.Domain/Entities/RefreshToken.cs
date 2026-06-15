using BuildingBlocks.Domain;

namespace Identity.Domain.Entities;

public class RefreshToken
{
  public Guid Id { get; private set; }

  public Guid UserId { get; private set; }

  public DateTimeOffset ExpiresAt { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public bool IsRevoked { get; private set; }

  public DateTimeOffset? RevokedAt { get; private set; }

  public string Token { get; private set; } = null!;

  private RefreshToken()
  {
  }

  private RefreshToken(
    Guid id,
    Guid userId,
    DateTimeOffset expiresAt,
    DateTimeOffset createdAt,
    bool isRevoked,
    DateTimeOffset? revokedAt)
  {
    Id = id;
    UserId = userId;
    ExpiresAt = expiresAt;
    CreatedAt = createdAt;
    IsRevoked = isRevoked;
    RevokedAt = revokedAt;
  }

  public static RefreshToken Create(
    Guid userId,
    DateTime expiresAt)
  {
    return new RefreshToken(Guid.NewGuid(), userId, expiresAt, DateTimeOffset.UtcNow, false, null);
  }

  public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

  public bool IsActive => !IsRevoked && !IsExpired;

  public void RevokeToken()
  {
    IsRevoked = true;
    RevokedAt = DateTime.UtcNow;
  }
}
