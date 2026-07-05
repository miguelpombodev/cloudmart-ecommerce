using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions;
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

  public async Task<User?> FindByEmail(string email) =>
    await _context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(user => user.Email.Address == email);

  public async Task<Role> FindRoleByName(string roleName, CancellationToken ct) =>
    await _context.Roles.SingleAsync(role => role.Name.Equals(roleName), ct)!;

  public async Task<RefreshToken> AddRefreshToken(RefreshToken token, CancellationToken ct)
  {
    EntityEntry<RefreshToken> resultStmt = await _context.RefreshTokens.AddAsync(token, ct);

    return resultStmt.Entity;
  }
}
