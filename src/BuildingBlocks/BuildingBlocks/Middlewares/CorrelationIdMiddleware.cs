using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BuildingBlocks.Middlewares;

/// <summary>
/// class to.
/// </summary>
public sealed class CorrelationIdMiddleware
{
  private const string CorrelationIdHeader = "X-Correlation-Id";

  private readonly RequestDelegate _next;

  /// <summary>
  ///   Initializes a new instance of the <see cref="CorrelationIdMiddleware" /> class.
  /// </summary>
  /// <param name="next">function to process the HTTP method.</param>
  public CorrelationIdMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  /// <summary>
  ///  execute the middleware.
  /// </summary>
  /// <param name="context">example.</param>
  /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
  public async Task InvokeAsync(HttpContext context)
  {
    string correlationId =
      context.Request.Headers[CorrelationIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString("N");

    context.Response.Headers[CorrelationIdHeader] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("HttpMethod", context.Request.Method))
    {
      await _next(context);
    }
  }
}
