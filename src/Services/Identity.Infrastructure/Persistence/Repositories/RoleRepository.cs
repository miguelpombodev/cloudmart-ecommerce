using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions.Repositories;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : RepositoryBase<Role, Guid, ApplicationDbContext>, IRoleRepository
{
  private readonly ApplicationDbContext _context;

  public RoleRepository(ApplicationDbContext ctx) : base(ctx)
  {
    _context = ctx;
  }

  public async Task<Role> FindRoleByName(string roleName, CancellationToken ct) =>
    await _context.Roles.SingleAsync(role => role.Name.Equals(roleName), ct)!;
}
