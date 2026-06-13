using BuildingBlocks.Domain;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public class Product : Aggregate<Guid>
{
  protected Product(
    string name,
    List<string> category,
    string description,
    List<Image> images,
    List<Review> reviews,
    Money price,
    int stockAmount)
  {
    Name = name;
    Category = category;
    Description = description;
    Images = images;
    Reviews = reviews;
    Price = price;
    StockAmount = stockAmount;
  }

  public string Name { get; private set; }

  public List<string> Category { get; private set; }

  public string Description { get; private set; }

  public List<Image>? Images { get; private set; }

  public List<Review> Reviews { get; private set; }

  public Money Price { get; private set; }

  public int StockAmount { get; private set; }
}
