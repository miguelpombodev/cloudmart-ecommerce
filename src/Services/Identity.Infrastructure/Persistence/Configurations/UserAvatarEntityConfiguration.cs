using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class UserAvatarEntityConfiguration : IEntityTypeConfiguration<UserAvatar>
{
  public void Configure(EntityTypeBuilder<UserAvatar> builder)
  {
    builder.ToTable("avatars", "identity");
    builder.HasKey(x => x.Id);

    builder.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500).IsRequired();
    builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
    builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
    builder.Property(rt => rt.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(rt => rt.UpdatedAt).HasColumnName("update_at").IsRequired();

    builder.HasIndex(x => x.UserId ).IsUnique();
  }
}
