using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : RepositoryBase<User, Guid, ApplicationDbContext>, IUserRepository
{
  private readonly ApplicationDbContext _context;

  public UserRepository(ApplicationDbContext ctx) : base(ctx)
  {
    _context = ctx;
  }

  public async Task<UserAuthenticationProvider> AddUserAuthenticationProviderAsync(
    UserAuthenticationProvider userAuthenticationProvider,
    CancellationToken ct)
  {
    EntityEntry<UserAuthenticationProvider> insertStmt =
      await _context.Set<UserAuthenticationProvider>().AddAsync(userAuthenticationProvider, ct);

    return insertStmt.Entity;
  }

  public async Task<UserAuthenticationProvider?> FindUserAuthenticationByProviderAsync(
    string emailAddress,
    string provider,
    CancellationToken ct)
  {
    UserAuthenticationProvider? result = await _context.Set<UserAuthenticationProvider>()
      .FirstOrDefaultAsync(
        uap => uap.Provider == provider.ToUpperInvariant() &&
               uap.Email.Address ==
               emailAddress,
        ct);

    return result;
  }

  public async Task<User?> FindByEmail(string email) =>
    await _context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(user => user.Email.Address == email);

  public async Task<RefreshToken> AddRefreshToken(RefreshToken token, CancellationToken ct)
  {
    EntityEntry<RefreshToken> resultStmt = await _context.RefreshTokens.AddAsync(token, ct);

    return resultStmt.Entity;
  }
}
