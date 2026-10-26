using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Catalog.Application.Features.Products.GetProducts;

public record GetProductsQuery(GetProductsRequest request) : IQuery<Result<IPaginatedResult<GetProductsResponse>>>;
