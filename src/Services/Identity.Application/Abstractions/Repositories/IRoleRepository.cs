using BuildingBlocks.Abstractions;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Repositories;

public interface IRoleRepository : IRepository<Role, Guid>
{
  Task<Role> FindRoleByName(string roleName, CancellationToken ct);
}
