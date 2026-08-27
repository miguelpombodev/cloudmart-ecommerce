using BuildingBlocks.Domain;

namespace Identity.Domain.Entities;

public sealed class UserAvatar : Entity<Guid>
{
  private UserAvatar()
  {
    AvatarImageName = null!;
    AvatarUrl = null!;
    AvatarContentType = null!;
  }

  private UserAvatar(
    string avatarUrl,
    Guid userId,
    string avatarImageName,
    string avatarContentType,
    bool isActive = true
  )
  {
    AvatarUrl = avatarUrl;
    AvatarImageName = avatarImageName;
    UserId = userId;
    IsActive = isActive;
    AvatarContentType = avatarContentType;
  }

  public string AvatarUrl { get; private set; }

  public string AvatarImageName { get; private set; }

  public string AvatarContentType { get; private set; }

  public Guid UserId { get; private set; }

  public bool IsActive { get; private set; }

  public static UserAvatar Create(
    string avatarUrl,
    Guid userId,
    string avatarFileName,
    string avatarContentType,
    bool isActive = true
  ) => new(avatarUrl, userId, avatarFileName, avatarContentType, isActive)
  {
    Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
  };

  public void Deactivate() =>
    IsActive = false;

  public void UpdateUrl(string newUrl) =>
    AvatarUrl = newUrl;

  public void UpdateFileName(string newFileName) =>
    AvatarImageName = newFileName;
}
