using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("users", "identity");
    builder.HasKey(u => u.Id);

    builder.ComplexProperty(
      u => u.Name,
      name =>
      {
        name.Property(n => n.FirstName)
          .HasColumnName("first_name")
          .HasMaxLength(100)
          .IsRequired();

        name.Property(n => n.LastName)
          .HasColumnName("last_name")
          .HasMaxLength(100)
          .IsRequired();

        name.Ignore(n => n.Initials);
      });

    builder.OwnsOne(
      u => u.Email,
      email =>
      {
        email.Property(e => e.Address)
          .HasColumnName("email")
          .HasMaxLength(320)
          .IsRequired();

        email.HasIndex(e => e.Address).IsUnique();
      });

    builder.OwnsOne(
      u => u.Password,
      password =>
      {
        password.Property(p => p.HashedValue)
          .HasColumnName("password_hash")
          .HasMaxLength(500);
      });

    builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
    builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(u => u.CreatedBy).HasColumnName("created_by").HasDefaultValue("user").IsRequired();
    builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").IsRequired();
    builder.Property(u => u.UpdatedBy).HasColumnName("updated_by").HasDefaultValue("user").IsRequired();

    builder.HasOne(u => u.Role).WithMany().HasForeignKey("role_id").IsRequired();

    builder.Metadata.FindNavigation(nameof(User.RefreshTokens))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    builder.HasMany(u => u.RefreshTokens).WithOne().HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(user => user.UserAuthenticationProviders).WithOne().HasForeignKey(uap => uap.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
