using BuildingBlocks.Abstractions;
using BuildingBlocks.Domain;
using Identity.Domain.ValueObject;

namespace Identity.Domain.Entities;

public class User : Aggregate<Guid>
{
  public CompleteName Name { get; }

  public Email Email { get; }

  public Password Password { get; }

  public Role Role { get; }

  public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

  public bool IsActive { get; private set; }

  private readonly List<RefreshToken> _refreshTokens = [];

  private User()
  {
    Name = null!;
    Email = null!;
    Password = null!;
    Role = null!;
  }

  private User(CompleteName name, Email email, Password password, Role role)
  {
    Name = name;
    Email = email;
    Password = password;
    Role = role;
    IsActive = true;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public static Result<User> Create(
    CompleteName name,
    Email email,
    Password password,
    Role role)
  {
    var user = new User(name, email, password, role) { Id = Guid.NewGuid() };

    return user;
  }

  public User Revoke(User user)
  {
    foreach (RefreshToken tokens in user.RefreshTokens)
    {
      tokens.RevokeToken();
    }

    return user;
  }

  public void AddRefreshToken(RefreshToken token) =>
    _refreshTokens.Add(token);
}
