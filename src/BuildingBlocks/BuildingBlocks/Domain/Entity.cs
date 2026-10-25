using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;

namespace BuildingBlocks.Domain;

public abstract class Entity<T> : IEntity<T>
{
	public required T Id { get; set; }

	public DateTimeOffset? CreatedAt { get; set; }

	public string? CreatedBy { get; set; }

	public DateTimeOffset? UpdatedAt { get; set; }

	public string? UpdatedBy { get; set; }

	public virtual void SetUpdateFieldsForNow(string whoDid)
	{
		if (string.IsNullOrEmpty(whoDid))
		{
			throw new DomainException(
				"A user tried to change a entity, however its name was passed as null for update field, please be assure of its name");
		}

		UpdatedAt = DateTimeOffset.UtcNow;
		UpdatedBy = whoDid;
	}
}
