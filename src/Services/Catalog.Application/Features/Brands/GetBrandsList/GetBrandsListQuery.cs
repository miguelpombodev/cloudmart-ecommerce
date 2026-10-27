using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;

namespace Catalog.Application.Features.Brands.GetBrandsList;

public sealed record GetBrandsListQuery(): IQuery<Result<GetBrandsListResponse>>;
