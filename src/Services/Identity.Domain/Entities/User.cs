using BuildingBlocks.Abstractions;
using BuildingBlocks.Domain;
using Identity.Domain.ValueObject;

namespace Identity.Domain.Entities;

public class User : Aggregate<Guid>
{
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

  public CompleteName Name { get; private set; }

  public Email Email { get; private set; }

  public Password Password { get; private set; }

  public Role Role { get; private set; }

  public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

  public bool IsActive { get; private set; }

  public static Result<User> Create(
    CompleteName name,
    Email email,
    Password password,
    Role role)
  {
    var user = new User(name, email, password, role) { Id = Guid.NewGuid() };

    return user;
  }

  public void Revoke()
  {
    foreach (RefreshToken tokens in _refreshTokens)
    {
      tokens.RevokeToken();
    }

    IsActive = false;
  }

  public void AddRefreshToken(RefreshToken token) =>
    _refreshTokens.Add(token);
}
