using Carter;
using Microsoft.AspNetCore.Antiforgery;

namespace Cloudmart.Identity.Features;

/// <inheritdoc />
public sealed class AntiForgery : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/antiforgery/token", (
        HttpContext context,
        IAntiforgery antiforgery) =>
      {
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);

        return Results.Ok(new { token = tokens.RequestToken });
      }).AllowAnonymous()
      .WithName("GetAntiForgeryToken")
      .Produces(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .WithSummary("Get AntiForgery Token for multipart-form");
  }
}
