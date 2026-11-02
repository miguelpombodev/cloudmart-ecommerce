using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Catalog.Application.Features.Products.GetProductsFiltersList;

public sealed record  GetProductsFiltersListQuery(GetProductsFilterListRequest Request): IQuery<Result<GetProductsFilterListResponse>>;
