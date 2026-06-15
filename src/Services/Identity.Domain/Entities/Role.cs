using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public class Role : Entity<Guid>
{
  public RoleType Name { get; }

  public string Description { get; }

  private Role()
  {
    Description = null!;
  }

  private Role(RoleType name, string description)
  {
    Description = description;
    Name = name;
  }

  public static Role Create(string description, RoleType name = RoleType.Customer)
  {
    if (string.IsNullOrWhiteSpace(description))
    {
      throw new DomainException("Token or Description cannot be null or whitespaced");
    }

    return new Role(name, description) { Id = Guid.NewGuid() };
  }
}
