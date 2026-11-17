using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class Category : Aggregate<Guid>
{
  private Category()
  {
    Title = null!;
    Image = null!;
  }

  private Category(Name title, Image slug)
  {
    Title = title;
    Image = slug;
  }

  public Name Title { get; private set; }

  public Image Image { get; private set; }

  public void SetName(Name title) =>
    Title = title ?? throw new DomainException("Category Title must not be a nullable value");

  public Category Create(Name title, Image image) =>
    new(title, image) { Id = Guid.NewGuid() };
}
