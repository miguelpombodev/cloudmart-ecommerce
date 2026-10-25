using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.ToTable("products", "catalog");
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).HasColumnName("id").HasColumnType("UUID").IsRequired();

    builder.OwnsOne(
      x => x.Title,
      t =>
      {
        t.Property(x => x.Value).HasColumnName("product_title").HasColumnType("VARCHAR(100)").IsRequired();
        t.HasIndex(x => x.Value).IsUnique();
      });

    builder.ComplexProperty(
      x => x.Price,
      m =>
      {
        m.Property(money => money.Amount).HasColumnName("price_amount").HasPrecision(18, 2);
        m.Property(money => money.Currency).HasColumnName("price_currency").HasMaxLength(3);
      });

    builder.Property(x => x.Sku).HasColumnName("product_sku").HasColumnType("VARCHAR(12)").IsRequired();
    builder.Property(x => x.CategoryId).HasColumnName("product_category_id").IsRequired();

    builder.Property(x => x.Description).HasColumnName("product_description").HasColumnType("VARCHAR(1200)")
      .IsRequired();

    builder.Property(x => x.Tags).HasColumnName("product_tags").HasColumnType("text[]");

    builder.OwnsMany(
      p => p.Images,
      img =>
      {
        img.ToTable("product_images");
        img.WithOwner().HasForeignKey("product_id");
        img.Property(i => i.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        img.Property(i => i.AltText).HasColumnName("alt_text").HasMaxLength(500);
        img.Property(i => i.IsPrimary).HasColumnName("is_primary");
      });

    builder.Navigation(p => p.Images).UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne<Brand>().WithMany().HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.Cascade);

    builder.Property(x => x.IsActive).HasColumnName("is_active").HasColumnType("BOOLEAN").IsRequired();

    builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
    builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
    builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

    builder.HasIndex(x => x.Sku).IsUnique();
  }
}
