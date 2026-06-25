using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.HasKey(r => r.Id);

    builder.Property(r => r.Name).HasColumnName("role_name").HasMaxLength(30).IsRequired();

    builder.Property(r => r.Type)
      .HasColumnName("role_type")
      .HasConversion(
        type => type.ToString(),
        dbStatus => (RoleType)Enum.Parse(typeof(RoleType), dbStatus));

    builder.Property(r => r.Description).HasColumnName("role_description").HasMaxLength(500).IsRequired();
    builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(r => r.CreatedBy).HasColumnName("created_by").IsRequired();
    builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();
    builder.Property(r => r.UpdatedBy).HasColumnName("updated_by").IsRequired();

    builder.HasIndex(r => r.Name).IsUnique();
  }
}
