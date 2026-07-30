using Carter;

namespace Cloudmart.Identity.Features.RefreshToken.Rotation;

/// <inheritdoc />
public class RefreshTokenRotationEndpoint : ICarterModule
{
  /// <inheritdoc />
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost()
  };
}
