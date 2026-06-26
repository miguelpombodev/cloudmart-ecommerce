using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Tests.Api.Common;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainerWrapper _dbContainer = new();

  public async Task InitializeAsync()
  {
    await _dbContainer.StartAsync();

    using IServiceScope scope = Services.CreateScope();
    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
  }

  public new async Task DisposeAsync() =>
    await _dbContainer.DisposeAsync();

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
      // Remove o DbContext registrado pelo Program.cs real (que apontaria
      // para o banco do Docker Compose) e registra um novo apontando
      // para o container efêmero do teste.
      ServiceDescriptor? descriptor =
        services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

      if (descriptor is not null)
      {
        services.Remove(descriptor);
      }

      services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(_dbContainer.ConnectionString));
    });
  }
}
