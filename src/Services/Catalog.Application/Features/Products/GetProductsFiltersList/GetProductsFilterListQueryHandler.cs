using BuildingBlocks.Abstractions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Domain;
using Catalog.Domain.Entities;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Features.Products.GetProductsFiltersList;

public sealed record ReviewStars(int Stars, int Count);

public sealed class
  GetProductsFilterListQueryHandler : IQueryHandler<GetProductsFiltersListQuery, Result<GetProductsFilterListResponse>>
{
  private readonly ApplicationDbContext _context;

  public GetProductsFilterListQueryHandler(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Result<GetProductsFilterListResponse>> Handle(
    GetProductsFiltersListQuery parameters,
    CancellationToken cancellationToken)
  {
    List<FacetItem> records = await BuildAndExecuteQuery(parameters.Request);

    List<FilterProp> brands = FetchCategoryInRecordsList(records, nameof(EFilterSections.Brands));

    List<FilterProp> categories = FetchCategoryInRecordsList(records, nameof(EFilterSections.Categories));

    List<FilterProp> reviews = FetchCategoryInRecordsList(records, nameof(EFilterSections.Reviews));

    var response = new GetProductsFilterListResponse(brands, categories, reviews);

    return Result<GetProductsFilterListResponse>.Success(response);
  }

  private List<FilterProp> FetchCategoryInRecordsList(List<FacetItem> records, string section)
  {
    return records.Where(record => record.Facet == section).Select(record =>
      new FilterProp { Id = record.Id, Title = record.Title, Quantity = record.Quantity }).ToList();
  }

  private IQueryable<Product> BuildBaseQuery(
    GetProductsFilterListRequest request
  )
  {
    IQueryable<Product> baseQuery = _context.Products.AsNoTracking();

    if (request.Brands?.Length > 0)
    {
      baseQuery = baseQuery.Where(p => request.Brands.Contains(p.BrandId));
    }

    if (request.Categories?.Length > 0)
    {
      baseQuery = baseQuery.Where(p => request.Categories.Contains(p.BrandId));
    }

    if (request.MinValue.HasValue)
    {
      baseQuery = baseQuery.Where(p => p.Price.Amount <= request.MinValue);
    }

    if (request.MaxValue.HasValue)
    {
      baseQuery = baseQuery.Where(p => p.Price.Amount >= request.MaxValue);
    }

    return baseQuery;
  }


  private async Task<List<FacetItem>> JoinTables(IQueryable<Product> baseQuery)
  {
    IQueryable<FacetItem> brands = baseQuery.Join(
        _context.Brands.AsNoTracking(),
        filteredProduct => filteredProduct.BrandId,
        brand => brand.Id,
        (p, b) => new { Facet = nameof(EFilterSections.Brands), Id = b.Id.ToString(), Title = b.Title.Value, })
      .GroupBy(b => new { b.Id, b.Title })
      .Select(g => new FacetItem { Facet = nameof(EFilterSections.Brands), Id = g.Key.Id, Title = g.Key.Title, Quantity = g.Count() });

    IQueryable<FacetItem> categories = baseQuery.Join(
        _context.Categories.AsNoTracking(),
        filteredProduct => filteredProduct.CategoryId,
        category => category.Id,
        (p, b) => new { Facet = nameof(EFilterSections.Categories), Id = b.Id.ToString(), Title = b.Title.Value, })
      .GroupBy(b => new { b.Id, b.Title })
      .Select(g => new FacetItem { Facet = nameof(EFilterSections.Categories), Id = g.Key.Id, Title = g.Key.Title, Quantity = g.Count() });

    List<FacetItem> rows = await brands
      .Concat(categories)
      .OrderBy(f => f.Facet)
      .ThenByDescending(f => f.Quantity)
      .ThenBy(f => f.Title)
      .Select(x => new FacetItem { Facet = x.Facet, Id = x.Id, Title = x.Title, Quantity = x.Quantity })
      .ToListAsync();

    var stock = await baseQuery
      .GroupBy(_ => 1)
      .Select(g => new { InStock = g.Count(p => p.StockQuantity > 0), OutOfStock = g.Count(p => p.StockQuantity <= 0) })
      .SingleOrDefaultAsync();

    var ratings = baseQuery
      .Join(
        _context.ProductReviews,
        filteredProduct => filteredProduct.Id,
        review => review.ProductId,
        (p, r) => r
      )
      .GroupBy(r => new { r.ProductId })
      .Select(g => new { g.Key.ProductId, Average = g.Average(x => x.Rating.Value) });

    var buckets = await ratings
      .GroupBy(_ => 1)
      .Select(g => new
      {
        S4 = g.Count(x => x.Average >= 4),
        S3 = g.Count(x => x.Average >= 3),
        S2 = g.Count(x => x.Average >= 2),
        S1 = g.Count(x => x.Average >= 1),
      })
      .SingleOrDefaultAsync();

    rows.Add(new FacetItem { Facet = "stock", Id = "in_stock", Title = "In Stock", Quantity = stock?.InStock ?? 0 });

    rows.Add(new FacetItem
    {
      Facet = "stock", Id = "out_of_stock", Title = "Out Of Stock", Quantity = stock?.OutOfStock ?? 0
    });

    ReviewStars[] thresholds =
    [
      new(4, buckets?.S4 ?? 0),
      new(3, buckets?.S3 ?? 0),
      new(2, buckets?.S2 ?? 0),
      new(1, buckets?.S1 ?? 0),
    ];

    rows.AddRange(thresholds.Select(t =>
      new FacetItem
      {
        Facet = nameof(EFilterSections.Reviews),
        Id = t.Stars.ToString(),
        Title = $"{t.Stars} estrelas ou mais",
        Quantity = t.Count
      }));

    return rows;
  }

  private static List<FacetItem> SortList(List<FacetItem> results) =>
    results
      .OrderBy(f => f.Facet)
      .ThenByDescending(f => f.Quantity)
      .ThenBy(f => f.Title)
      .ToList();


  private async Task<List<FacetItem>> BuildAndExecuteQuery(
    GetProductsFilterListRequest request
  )
  {
    IQueryable<Product> baseQuery = BuildBaseQuery(request);

    List<FacetItem> results = await JoinTables(baseQuery);

    List<FacetItem> sortedList = SortList(results);

    return sortedList;
  }
}
