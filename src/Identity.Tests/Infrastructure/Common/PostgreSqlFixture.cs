using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Identity.Tests.Infrastructure.Common;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
#pragma warning disable CS0618 // Type or member is obsolete
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
#pragma warning restore CS0618 // Type or member is obsolete
    .WithImage("postgres:16-alpine")
    .WithDatabase("IdentityDB_Test")
    .WithUsername("identity_test")
    .WithPassword("test_password")
    .Build();

  public string ConnectionString => _container.GetConnectionString();

  // Guarda a Role seedada para os testes reutilizarem
  public Role DefaultCustomerRole { get; private set; } = null!;

  public async Task InitializeAsync()
  {
    await _container.StartAsync();

    DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseNpgsql(ConnectionString)
      .Options;

    await using var context = new ApplicationDbContext(options);
    await context.Database.MigrateAsync();

    DefaultCustomerRole = Role.Create("Default customer role");
    context.Roles.Add(DefaultCustomerRole);
    await context.SaveChangesAsync();
  }

  public async Task DisposeAsync() =>
    await _container.DisposeAsync();

  /// <summary>
  ///   Cria um novo DbContext apontando para o container.
  ///   Um novo DbContext por teste (não compartilhado) evita que o
  ///   Change Tracker de um teste influencie o próximo.
  /// </summary>
  public ApplicationDbContext CreateContext()
  {
    DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseNpgsql(ConnectionString)
      .Options;

    return new ApplicationDbContext(options);
  }
}
