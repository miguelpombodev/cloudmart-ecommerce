using BuildingBlocks.Domain;
using Identity.Domain.ValueObject;

namespace Identity.Domain.Entities;

public sealed class User : Aggregate<Guid>
{
  private readonly List<RefreshToken> _refreshTokens = [];

  private readonly List<UserAuthenticationProvider> _userAuthenticationProviders = [];

  private User()
  {
    Name = null!;
    Email = null!;
    Password = null!;
    Role = null!;
  }

  private User(CompleteName name, Email email, Password? password, Role role)
  {
    Name = name;
    Email = email;
    Password = password;
    Role = role;
    IsActive = true;
    CreatedAt = DateTimeOffset.UtcNow;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  public CompleteName Name { get; private set; }

  public Email Email { get; }

  public Password? Password { get; private set; }

  public Role Role { get; private set; }

  public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

  public IReadOnlyList<UserAuthenticationProvider> UserAuthenticationProviders =>
    _userAuthenticationProviders.AsReadOnly();

  public bool IsActive { get; private set; }

  public static User Create(
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
    foreach (RefreshToken tokens in _refreshTokens.Where(token => token.RevokedAt is null))
    {
      tokens.RevokeToken();
    }

    IsActive = false;
  }

  public string RetrieveMaskedEmail()
  {
    string address = Email.Address;

    return $"{address.Substring(address.Length - 4)}{new string('*', address.Length)}";
  }

  public void AddRefreshToken(RefreshToken token) =>
    _refreshTokens.Add(token);
}
