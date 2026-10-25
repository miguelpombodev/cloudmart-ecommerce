using BuildingBlocks.Domain;
using Catalog.Domain.ValueObject;

namespace Catalog.Domain.Entities;

public sealed class ProductReview : Aggregate<Guid>
{
	private ProductReview()
	{
		Contents = null!;
		Description = null!;
    Rating = null!;
  }

	private ProductReview(Guid productId, Rate rating, IList<string> contents, string description, Guid userId)
	{
		ProductId = productId;
		Rating = rating;
		Contents = contents;
		Description = description;
		UserId = userId;
	}

	public Guid ProductId { get; private set; }

	public Rate Rating { get; private set; }

	public IList<string> Contents { get; private set; }

	public string Description { get; private set; }

	public Guid UserId { get; private set; }

	public ProductReview Create(
		Guid productId,
		string description,
		Guid userId,
		Rate rating)
	{
		return new ProductReview(
			productId,
			rating,
			[],
			description,
			userId
		) {Id = Guid.NewGuid()};
	}
}
