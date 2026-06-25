using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Domain;

public abstract class Entity<T> : IEntity<T>
{
  public required T Id { get; set; }

  public DateTimeOffset? CreatedAt { get; set; }

  public string? CreatedBy { get; set; }

  public DateTimeOffset? UpdatedAt { get; set; }

  public string? UpdatedBy { get; set; }
}
