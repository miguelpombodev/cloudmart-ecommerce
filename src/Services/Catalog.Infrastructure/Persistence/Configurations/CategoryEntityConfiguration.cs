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

    builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
    builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();
  }
}
