using BuildingBlocks.Abstractions;
using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Domain.ValueObject;
using Identity.Tests.Domain.Builder;

namespace Identity.Tests.Domain.Entities;

public class UserTests
{
  [Fact]
  public void Create_WithValidData_ShouldReturnSuccessResult()
  {
    // Arrange
    var name = CompleteName.Create("João", "Silva");
    var email = Email.Create("joao@example.com");
    var password = Password.CreateWithNoProvider("Senha@123");
    var role = Role.Create("Default role");

    // Act
    Result<User> result = User.Create(name, email, password, role);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(name);
    result.Value.Email.Should().Be(email);
  }

  [Fact]
  public void Create_ShouldGenerateWithNonEmptyId()
  {
    User user = new UserBuilder().Build();

    user.Id.Should().NotBe(Guid.Empty);
  }

  [Fact]
  public void Create_NewUser_ShouldBeActiveByDefault()
  {
    User user = new UserBuilder().Build();

    user.IsActive.Should().BeTrue();
  }

  [Fact]
  public void Create_NewUser_ShouldSetCreateAtToUtcNow()
  {
    DateTimeOffset before = DateTimeOffset.UtcNow;

    User user = new UserBuilder().Build();

    DateTimeOffset after = DateTimeOffset.UtcNow;

    user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
  }

  [Fact]
  public void AddRefreshToken_ShouldAddTokenCollection()
  {
    User user = new UserBuilder().Build();
    var token = RefreshToken.Create(user.Id, "", DateTimeOffset.UtcNow.AddDays(7));

    user.AddRefreshToken(token);

    user.RefreshTokens.Should().ContainSingle().Which.Should().Be(token);
  }

  [Fact]
  public void Revoke_WithMultipleActiveTokens_ShouldRevokeAllOfThem()
  {
    // Arrange
    User user = new UserBuilder().Build();
    var tokenA = RefreshToken.Create(user.Id, "", DateTimeOffset.UtcNow.AddDays(7));
    var tokenB = RefreshToken.Create(user.Id, "", DateTimeOffset.UtcNow.AddDays(7));
    var tokenC = RefreshToken.Create(user.Id, "", DateTimeOffset.UtcNow.AddDays(7));

    // Act
    user.AddRefreshToken(tokenA);
    user.AddRefreshToken(tokenB);
    user.AddRefreshToken(tokenC);

    user.Revoke();

    // Assert
    user.RefreshTokens.Should().AllSatisfy(token =>
      token.IsRevoked.Should().BeTrue());
  }

  [Fact]
  public void Revoke_WithNoTokens_ShouldNotThrow()
  {
    User user = new UserBuilder().Build();

    Action act = () => user.Revoke();

    act.Should().NotThrow();
  }

  [Fact]
  public void Revoke_ShouldNotAffectAlreadyRevokedTokens()
  {
    User user = new UserBuilder().Build();
    var token = RefreshToken.Create(user.Id, "", DateTimeOffset.UtcNow.AddDays(7));
    user.AddRefreshToken(token);

    user.Revoke();
    DateTimeOffset? firstRevokedAt = token.RevokedAt;

    user.Revoke();
    DateTimeOffset? secondRevokedAt = token.RevokedAt;
    firstRevokedAt.Should().Be(secondRevokedAt);
  }

  [Fact]
  public void TwoUsersWithDifferentIds_ShouldNotBeConsideredTheSameEntity()
  {
    User userA = new UserBuilder().WithEmail("a@example.com").Build();
    User userB = new UserBuilder().WithEmail("a@example.com").Build();

    userA.Id.Should().NotBe(userB.Id);
  }
}
