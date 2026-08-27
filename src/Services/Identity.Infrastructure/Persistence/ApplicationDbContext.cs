using System.Reflection;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  public DbSet<User> Users => Set<User>();

  public DbSet<Role> Roles => Set<Role>();

  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  public DbSet<UserAvatar> UserAvatars => Set<UserAvatar>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    base.OnModelCreating(modelBuilder);
  }
}
