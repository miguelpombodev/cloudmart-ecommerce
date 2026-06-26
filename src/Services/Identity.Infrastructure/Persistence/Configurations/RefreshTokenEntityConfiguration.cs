using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    builder.ToTable("refresh_tokens", "identity");
    builder.HasKey(rt => rt.Id);

    builder.Property(rt => rt.Token).HasColumnName("token_value").HasMaxLength(500).IsRequired();
    builder.Property(rt => rt.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false).IsRequired();
    builder.Property(rt => rt.UserId).HasColumnName("user_id").IsRequired();
    builder.Property(rt => rt.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(rt => rt.ExpiresAt).HasColumnName("expires_at").IsRequired();

    builder.HasIndex(rt => rt.Token);
  }
}
