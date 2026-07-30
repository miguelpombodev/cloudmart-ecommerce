using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public sealed class Role : Aggregate<Guid>
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
  }

  public string Name { get; private set; }

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

  public static Role CreateSeed(Guid id, string name, string description, RoleType type)
  {
    return new Role
    {
      Id = id,
      Name = name,
      Description = description,
      Type = type,
      CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
      UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
      CreatedBy = "system",
      UpdatedBy = "system"
    };
  }
}
