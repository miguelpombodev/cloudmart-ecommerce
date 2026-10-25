using System.Reflection;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
  { }

  public DbSet<Product> Products => Set<Product>();
  public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    base.OnModelCreating(modelBuilder);
  }
}
