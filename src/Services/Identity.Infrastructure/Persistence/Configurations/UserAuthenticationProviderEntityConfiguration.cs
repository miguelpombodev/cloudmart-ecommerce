using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class UserAuthenticationProviderEntityConfiguration : IEntityTypeConfiguration<UserAuthenticationProvider>
{
  private const string ProviderSqlIndexName = "IX_UserAuthenticationProvider_ProviderName";

  public void Configure(EntityTypeBuilder<UserAuthenticationProvider> builder)
  {
    builder.ToTable("user_oauth_providers");
    builder.HasKey(uap => uap.Id);

    builder.Property(uap => uap.Id).HasColumnName("id").IsRequired();
    builder.Property(uap => uap.UserId).HasColumnName("user_id").IsRequired();
    builder.Property(uap => uap.Provider).HasColumnName("provider_name").HasColumnType("VARCHAR(20)").IsRequired();
    builder.Property(uap => uap.ProviderUserId).HasColumnName("provider_user_id").IsRequired();
    builder.Property(uap => uap.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(uap => uap.UpdatedAt).HasColumnName("updated_at").IsRequired();
    builder.Property(uap => uap.CreatedBy).HasColumnName("created_by").HasDefaultValue("user").IsRequired();
    builder.Property(uap => uap.UpdatedBy).HasColumnName("updated_by").HasDefaultValue("user").IsRequired();

    builder.OwnsOne(
      u => u.Email,
      email =>
      {
        email.Property(e => e.Address)
          .HasColumnName("email")
          .HasMaxLength(320)
          .IsRequired();
      });

    builder.HasIndex(uap => uap.Provider, ProviderSqlIndexName).IsUnique();
  }
}
