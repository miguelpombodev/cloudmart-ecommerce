using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.Tests.Api.Common;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainerWrapper _dbContainer = new();

  private readonly RabbitMqContainerWrapper _rabbitMqContainer = new();

  public async Task InitializeAsync()
  {
    await _dbContainer.StartAsync();
    await _rabbitMqContainer.StartAsync();

    using IServiceScope scope = Services.CreateScope();
    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
  }

  public new async Task DisposeAsync() =>
    await _dbContainer.DisposeAsync();

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureAppConfiguration((_, configuration) =>
    {
      var settings = new Dictionary<string, string?>
      {
        ["ConnectionStrings:Database"] = _dbContainer.ConnectionString,
        ["Authentication:Google:Url"] =
          "https://oauth2.googleapis.com/",
        ["Authentication:Google:ClientId"] =
          "testing_client_id",
        ["Authentication:Google:ClientSecret"] =
          "testing_client_secret",
        ["Authentication:Google:TimeoutInMinutes"] = "1",
        ["Jwt:SecretKey"] =
          "test-only-secret-key-with-at-least-32-chars!",
        ["Jwt:Issuer"] = "thistest.com",
        ["Jwt:Audience"] = "thistest.com",
        ["Jwt:ExpirationMinutes"] = "15",
        ["MessageBroker:Host"] = _rabbitMqContainer.RetrieveHostName(),
        ["MessageBroker:Username"] = "test",
        ["MessageBroker:Password"] = "test-password"
      };

      configuration.AddInMemoryCollection(settings);
    });

    builder.ConfigureServices(services =>
    {
      // Remove o DbContext registrado pelo Program.cs real (que apontaria
      // para o banco do Docker Compose) e registra um novo apontando
      // para o container efêmero do teste.
      services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

      services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(_dbContainer.ConnectionString));
    });
  }
}
