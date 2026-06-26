using BuildingBlocks.Infrastructure;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : RepositoryBase<User, Guid, ApplicationDbContext>, IUserRepository
{

  public UserRepository(ApplicationDbContext ctx) : base(ctx)
  {
  }
}
