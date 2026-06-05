using BuildingBlocks.Abstractions;

namespace Catalog.Domain.Products.Entities;

public class Product : Aggregate<Guid>
{
	public string Name { get; set; } = default!;
	public List<string> Category { get; set; } = default!;
	public string Description { get; set; } = default!;
	public string ImageFile { get; set; } = default!;
	public decimal Price { get; set; }
	public int StockAmount { get; set; }
}
