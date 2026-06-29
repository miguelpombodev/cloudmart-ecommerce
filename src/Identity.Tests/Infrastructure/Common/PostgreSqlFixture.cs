using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Identity.Tests.Infrastructure.Common;

public sealed class PostgreSqlFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
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

    DefaultCustomerRole = Role.CreateSeed(Guid.Parse("00000000-0000-0000-0000-000000000001"),"Test role", "Test role", RoleType.Customer);
    context.Roles.Add(DefaultCustomerRole);
    await context.SaveChangesAsync();
  }

  public new async Task DisposeAsync() =>
    await _container.StopAsync();

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

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      ServiceDescriptor? descriptor =
        services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

      if (descriptor is not null)
      {
        services.Remove(descriptor);
      }

      services.AddDbContext<ApplicationDbContext>(options => { options.UseNpgsql(_container.GetConnectionString()); });
    });
  }
}
