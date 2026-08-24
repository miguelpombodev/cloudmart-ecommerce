using System.Security.Claims;
using BuildingBlocks.Abstractions;

namespace Cloudmart.Identity.Services;

/// <summary>
///   assasasasaa.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
  private readonly IHttpContextAccessor httpContextAccessor;

  /// <summary>
  ///   Initializes a new instance of the <see cref="CurrentUser" /> class.
  ///   adasdasdasa.
  /// </summary>
  /// <param name="httpContextAccessor">asdasdasdas.</param>
  public CurrentUser(IHttpContextAccessor httpContextAccessor)
  {
    this.httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc />
  public Guid UserId =>
    Guid.TryParse(
      httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
      out Guid userId)
      ? userId
      : Guid.Empty;

  /// <inheritdoc />
  public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

  /// <inheritdoc />
  public string Email =>
    httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

  /// <inheritdoc />
  public List<string> Roles =>
    httpContextAccessor.HttpContext?.User
      .FindAll(ClaimTypes.Role)
      .Select(c => c.Value)
      .ToList() ??
    new List<string>();
}
