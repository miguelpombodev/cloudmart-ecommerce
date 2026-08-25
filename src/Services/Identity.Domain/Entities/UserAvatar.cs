using BuildingBlocks.Domain;

namespace Identity.Domain.Entities;

public sealed class UserAvatar : Entity<Guid>
{
  private UserAvatar()
  {
    AvatarUrl = null!;
  }

  private UserAvatar(
    string avatarUrl,
    Guid userId,
    bool isActive = true
  )
  {
    AvatarUrl = avatarUrl;
    UserId = userId;
    IsActive = isActive;
  }

  public string AvatarUrl { get; private set; }

  public Guid UserId { get; private set; }

  public bool IsActive { get; private set; }

  public static UserAvatar Create(
    string avatarUrl,
    Guid userId,
    bool isActive
  ) => new(avatarUrl, userId, isActive) { Id = Guid.NewGuid() };

  public void Deactivate() =>
    IsActive = false;

  public void UpdateUrl(string newUrl) =>
    AvatarUrl = newUrl;
}
