using BuildingBlocks.Domain;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class Brand : Aggregate<Guid>
{
	private Brand()
	{
		Title = null!;
	}

	private Brand(Name title)
	{
		Title = title;
	}

	public Name Title { get; private set; }

	public Brand Create(Name title)
	{
		return new Brand(title) { Id = Guid.NewGuid() };
	}
}
