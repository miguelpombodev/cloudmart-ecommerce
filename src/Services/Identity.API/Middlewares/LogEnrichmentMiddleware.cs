using BuildingBlocks.Abstractions;
using Serilog.Context;

namespace Cloudmart.Identity.Middlewares;

/// <summary>
///   asasasasasa.
/// </summary>
public sealed class LogEnrichmentMiddleware
{
  private readonly RequestDelegate next;

  /// <summary>
  ///   Initializes a new instance of the <see cref="LogEnrichmentMiddleware" /> class.
  ///   Asasasaaasaa.
  /// </summary>
  /// <param name="next">asasasaasasa.</param>
  public LogEnrichmentMiddleware(RequestDelegate next)
  {
    this.next = next;
  }

  /// <summary>
  ///   asasss.
  /// </summary>
  /// <param name="context">assa.</param>
  /// <param name="currentUser">asa.</param>
  /// <returns>asaas.</returns>
  public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
  {
    using (LogContext.PushProperty("UserId", currentUser.UserId))
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("HttpMethod", context.Request.Method))
    {
      await next(context);
    }
  }
}
