using BuildingBlocks.Abstractions;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IUserRepository : IRepository<User, Guid>
{
  Task<UserAuthenticationProvider> AddUserAuthenticationProviderAsync(
    UserAuthenticationProvider userAuthenticationProvider,
    CancellationToken ct);

  Task<UserAuthenticationProvider?> FindUserAuthenticationByProviderAsync(
    string emailAddress,
    string provider,
    CancellationToken ct);

  Task<User?> FindByEmail(string email);
  Task<Role> FindRoleByName(string roleName, CancellationToken ct);
  Task<RefreshToken> AddRefreshToken(RefreshToken token, CancellationToken ct);
}
