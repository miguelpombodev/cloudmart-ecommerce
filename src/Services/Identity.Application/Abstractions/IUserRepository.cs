using BuildingBlocks.Abstractions;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IUserRepository : IRepository<User, Guid>
{
  Task<User?> FindByEmail(string email);
}
