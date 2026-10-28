using BuildingBlocks.Domain;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class Brand : Aggregate<Guid>
{
  private Brand()
  {
    Title = null!;
    ImageUrl = null!;
  }

  private Brand(Name title, string imageUrl)
  {
    Title = title;
    ImageUrl = imageUrl;
  }

  public Name Title { get; private set; }

  public string ImageUrl { get; private set; }

  public Brand Create(Name title, string imageUrl)
  {
    return new Brand(title, imageUrl) { Id = Guid.NewGuid() };
  }
}
