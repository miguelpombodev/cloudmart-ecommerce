using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using Catalog.Domain.Entities;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Features.Brands.GetBrandsList;

public sealed class GetBrandsListQueryHandler : IQueryHandler<GetBrandsListQuery, Result<GetBrandsListResponse>>
{
  private readonly ApplicationDbContext _context;

  public GetBrandsListQueryHandler(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Result<GetBrandsListResponse>> Handle(
    GetBrandsListQuery request,
    CancellationToken cancellationToken)
  {
    List<BrandDto> brands = await BuildQueryAndExecute();

    var response = new GetBrandsListResponse(brands);

    return Result<GetBrandsListResponse>.Success(response);
  }

  private async Task<List<BrandDto>> BuildQueryAndExecute()
  {
    return await _context
      .Set<Brand>()
      .AsNoTracking()
      .Select(brand => new BrandDto { Name = brand.Title.Value })
      .ToListAsync();
  }
}
