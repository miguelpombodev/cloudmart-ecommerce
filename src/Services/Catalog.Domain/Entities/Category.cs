using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class Category : Aggregate<Guid>
{
	private Category()
	{
		Title = null!;
    Slug = null!;
  }

	private Category(Name title, string slug)
	{
		Title = title;
    Slug = slug;
  }

  public Name Title { get; private set; }
	public string Slug { get; private set; }

	public void SetName(Name title)
	{
		Title = title ?? throw new DomainException("Category Title must not be a nullable value");
	}

	public Category Create(Name title, string slug)
	{
		return new Category(title, slug) { Id = Guid.NewGuid() };
	}
}
