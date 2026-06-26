using Testcontainers.PostgreSql;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Identity.Tests.Api.Common;

/// <summary>
///   Wrapper fino sobre o container — extraído para evitar duplicar a
///   configuração do container entre PostgreSqlFixture (Integration) e
///   IdentityApiFactory (E2E). Ambos precisam de "um PostgreSQL real
///   efêmero", só o CONSUMO desse container é diferente entre as duas
///   camadas de teste.
/// </summary>
public sealed class PostgreSqlContainerWrapper : IAsyncDisposable
{
  private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("IdentityDB_E2E")
    .WithUsername("identity_e2e")
    .WithPassword("e2e_password")
    .Build();

  public string ConnectionString =>
    _container.GetConnectionString();

  public async ValueTask DisposeAsync() =>
    await _container.DisposeAsync();

  public Task StartAsync() =>
    _container.StartAsync();
}
