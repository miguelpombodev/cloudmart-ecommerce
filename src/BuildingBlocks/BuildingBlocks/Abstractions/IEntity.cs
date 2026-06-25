namespace BuildingBlocks.Abstractions;

public interface IEntity<T> : IEntity
{
  T Id { get; set; }
}

public interface IEntity
{
  DateTimeOffset? CreatedAt { get; set; }

  string? CreatedBy { get; set; }

  DateTimeOffset? UpdatedAt { get; set; }

  string? UpdatedBy { get; set; }
}
