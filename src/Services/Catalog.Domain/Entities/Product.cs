using BuildingBlocks.Domain;
using BuildingBlocks.Exceptions;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class Product : Aggregate<Guid>
{
  private readonly List<Image> _images = [];

  private Product()
  {
    Title = null!;
    Sku = null!;
    Description = null!;
    Price = null!;
    Tags = null!;
  }

  private Product(
    Name title,
    string sku,
    Guid categoryId,
    string description,
    List<string> tags,
    Guid brandId,
    Money price,
    int stockQuantity,
    bool isActive)
  {
    Title = title;
    Sku = sku;
    CategoryId = categoryId;
    Description = description;
    Price = price;
    StockQuantity = stockQuantity;
    BrandId = brandId;
    Tags = tags;
    IsActive = isActive;
  }

  public Name Title { get; private set; }

  public string Sku { get; private set; }

  public string Description { get; private set; }

  public List<string> Tags { get; private set; }

  public Guid CategoryId { get; private set; }

  public Money Price { get; private set; }

  public Guid BrandId { get; private set; }

  public int StockQuantity { get; private set; }

  public bool IsActive { get; private set; }

  public IReadOnlyList<Image> Images => _images.AsReadOnly();

  public static Product Create(
    Name title,
    string sku,
    Guid categoryId,
    string description,
    List<string> tags,
    Guid brandId,
    Money price,
    int stockQuantity = 0,
    bool isActive = true)
  {
    return new Product(
      title,
      sku,
      categoryId,
      description,
      tags,
      brandId,
      price,
      stockQuantity,
      isActive
    ) { Id = Guid.NewGuid() };
  }

  public void AddImage(Image image)
  {
    if (image.IsPrimary && _images.Any(i => i.IsPrimary))
    {
      throw new DomainException("Product already has a primary image");
    }

    _images.Add(image);
  }
}
