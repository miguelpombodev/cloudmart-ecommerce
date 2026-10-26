using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductReviewEntityConfiguration : IEntityTypeConfiguration<ProductReview>
{
	public void Configure(EntityTypeBuilder<ProductReview> builder)
	{
		builder.ToTable("product_review", "catalog");
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id).HasColumnName("id").HasColumnType("UUID").IsRequired();
		builder.Property(x => x.ProductId).HasColumnName("review_product_id").HasColumnType("UUID").IsRequired();

    builder.OwnsOne(
      x => x.Rating,
      rate =>
      {
        rate.Property(x => x.Value).HasColumnName("review_rating").HasColumnType("NUMERIC(2,1)").IsRequired();
        rate.HasIndex(x => x.Value).IsDescending();
      });

		builder.Property(x => x.Contents).HasColumnName("review_contents").HasColumnType("text[]").IsRequired();
		builder.Property(x => x.Description).HasColumnName("review_description").HasColumnType("VARCHAR(800)").IsRequired();
		builder.Property(x => x.UserId).HasColumnName("review_user_id").HasColumnType("UUID").IsRequired();
		builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
		builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

		builder.Ignore(x => x.CreatedBy);
		builder.Ignore(x => x.UpdatedBy);

		builder.HasIndex(x => new { x.UserId, x.ProductId });
	}
}
