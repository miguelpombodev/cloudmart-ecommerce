namespace BuildingBlocks.Abstractions;

public interface IEntity<T> : IEntity
{
	T Id { get; set; }
}

public interface IEntity
{
	DateTime? CreatedAt { get; set; }
	string? CreatedBy { get; set; }
	DateTime? UpdatedAt { get; set; }
	string? UpdatedBy { get; set; }
}
