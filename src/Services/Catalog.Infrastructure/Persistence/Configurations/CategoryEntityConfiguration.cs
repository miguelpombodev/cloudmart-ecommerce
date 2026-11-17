using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.ToTable("categories", "catalog");
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("UUID").IsRequired();

    builder.OwnsOne(
      x => x.Title,
      category =>
      {
        category.Property(x => x.Value).HasColumnName("category_title").HasColumnType("VARCHAR(100)").IsRequired();
        category.HasIndex(x => x.Value).IsUnique();
      });

    builder.OwnsOne(
      x => x.Image,
      category =>
      {
        category.Property(x => x.Url).HasColumnName("category_image_url").HasMaxLength(2048).IsRequired();
        category.Property(x => x.IsPrimary).HasColumnName("category_image_is_primary");
        category.Property(x => x.AltText).HasColumnName("category_image_alt_text").HasMaxLength(500);
        category.HasIndex(x => x.Url);
      });

    builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
    builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();
  }
}
