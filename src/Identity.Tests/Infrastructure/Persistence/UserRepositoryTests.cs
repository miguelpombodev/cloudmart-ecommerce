using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Tests.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Identity.Tests.Infrastructure.Persistence;

public sealed class UserRepositoryTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
  private readonly PostgreSqlFixture _fixture;

  public UserRepositoryTests(PostgreSqlFixture fixture)
  {
    _fixture = fixture;
  }

  // Roda ANTES de cada teste individual — diferente da Fixture, que
  // roda uma vez para a CLASSE inteira. IAsyncLifetime pode ser
  // implementado na própria classe de teste para esse propósito.
  public async Task InitializeAsync()
  {
    await using ApplicationDbContext context = _fixture.CreateContext();

    // Limpa as tabelas relevantes antes de cada teste, mantendo a
    // Role seedada intacta (ela é seedada uma vez na Fixture).
    await context.Database.ExecuteSqlRawAsync("DELETE FROM identity.users");
  }

  public Task DisposeAsync() =>
    Task.CompletedTask;


  [Fact]
  public async Task FindByEmail_WhenUserDoesNotExist_ShouldReturnNull()
  {
    await using ApplicationDbContext context = _fixture.CreateContext();
    var repository = new UserRepository(context);

    User? found = await repository.FindByEmail("nonexistent@example.com");

    found.Should().BeNull();
  }
}
