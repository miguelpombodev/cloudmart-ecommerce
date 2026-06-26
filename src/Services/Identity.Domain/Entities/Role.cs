using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public class Role : Entity<Guid>
{
  private Role()
  {
    Name = null!;
    Description = null!;
  }

  private Role(string name, string description, RoleType roleType)
  {
    Description = description;
    Name = name;
    Type = roleType;
    CreatedAt = DateTimeOffset.UtcNow;
    CreatedBy = "system";
    UpdatedAt = DateTimeOffset.UtcNow;
    UpdatedBy = "system";
  }

  public string Name { get; set; }

  public string Description { get; private set; }

  public RoleType Type { get; private set; }


  public static Role Create(string description, RoleType type = RoleType.Customer)
  {
    if (string.IsNullOrWhiteSpace(description))
    {
      throw new DomainException("Token or Description cannot be null or whitespaced");
    }

    return new Role(type.ToString(), description, type) { Id = Guid.NewGuid() };
  }
}
