using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class Category : Aggregate<Guid>
{
	private Category()
	{
		Title = null!;
	}

	private Category(Name title)
	{
		Title = title;
	}

	public Name Title { get; private set; }

	public void SetName(Name title)
	{
		Title = title ?? throw new DomainException("Category Title must not be a nullable value");
	}

	public Category Create(Name title)
	{
		return new Category(title) { Id = Guid.NewGuid() };
	}
}
