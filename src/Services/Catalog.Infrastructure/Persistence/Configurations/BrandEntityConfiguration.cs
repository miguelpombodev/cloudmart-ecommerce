using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class BrandEntityConfiguration : IEntityTypeConfiguration<Brand>
{
	public void Configure(EntityTypeBuilder<Brand> builder)
	{
		builder.ToTable("brands", "catalog");
		builder.HasIndex(x => x.Id);

		builder.Property(x => x.Id).HasColumnName("id").HasColumnType("UUID").IsRequired();

		builder.OwnsOne(
			x => x.Title,
			b =>
			{
				b.Property(name => name.Value).HasColumnName("brand_title").HasColumnType("VARCHAR(100)").IsRequired();
		    b.HasIndex(name => name.Value).IsUnique();
			});

		builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
		builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
		builder.Property(x => x.CreatedBy).HasColumnName("created_by").IsRequired();
		builder.Property(x => x.UpdatedBy).HasColumnName("updated_by").IsRequired();

	}
}
