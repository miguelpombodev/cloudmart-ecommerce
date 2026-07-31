using BuildingBlocks.Abstractions;
using Serilog.Context;

namespace Cloudmart.Identity.Middlewares;

/// <summary>
///   asasasasasa.
/// </summary>
public sealed class LogEnrichmentMiddleware
{
  private readonly RequestDelegate _next;

  /// <summary>
  ///   Initializes a new instance of the <see cref="LogEnrichmentMiddleware" /> class.
  ///   Asasasaaasaa.
  /// </summary>
  /// <param name="next">asasasaasasa.</param>
  public LogEnrichmentMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  /// <summary>
  ///   asasss.
  /// </summary>
  /// <param name="context">assa.</param>
  /// <param name="currentUser">asa.</param>
  /// <returns>asaas.</returns>
  public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
  {
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("HttpMethod", context.Request.Method))
    using (LogContext.PushProperty("ClientIp", context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown"))
    {
      // UserId só existe em endpoints autenticados
      // Guid.Empty significa requisição anônima (login, register, refresh)
      if (currentUser.UserId != Guid.Empty)
      {
        using (LogContext.PushProperty("UserId", currentUser.UserId))
        {
          await _next(context);

          return;
        }
      }

      await _next(context);
    }
  }
}
