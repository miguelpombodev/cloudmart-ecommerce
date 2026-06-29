using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
  private readonly Role[] _seed = new[]
  {
    Role.CreateSeed(
      Guid.Parse("f8f4e2b8-7351-4e7f-a5b0-69f8f66940e4"),
      "Customer",
      "Customer role",
      RoleType.Customer),
    Role.CreateSeed(Guid.Parse("1629dfbc-df35-4aed-a071-08755df25354"), "Admin", "Admin role", RoleType.Admin),
    Role.CreateSeed(Guid.Parse("19804ec2-fe97-4a75-b084-035eb84fdeb5"), "Member", "Member role", RoleType.Member),
    Role.CreateSeed(Guid.Parse("37d43ef9-6739-40d9-b876-6ff9e92e68d4"), "Manager", "Manager role", RoleType.Manager),
    Role.CreateSeed(Guid.Parse("d7d42ee4-3899-4630-9ecc-6eccc8af04a7"), "Seller", "Seller role", RoleType.Seller),
    Role.CreateSeed(Guid.Parse("b97dd358-e092-43c4-91c4-f34f2b04927c"), "Support", "Support role", RoleType.Support)
  };

  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.ToTable("roles", "identity");
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

    builder.HasData(_seed);
  }
}
