using BuildingBlocks.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Cloudmart.Identity.Middlewares;

/// <inheritdoc />
public sealed class GlobalExceptionHandlerMiddleware : IMiddleware
{
  private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
  /// class GlobalExceptionHandlerMiddleware.
  /// </summary>
  /// <param name="logger"> logger param.</param>
  public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
  {
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    try
    {
      await next(context);
    }
    catch (ValidationException ex)
    {
      _logger.LogWarning("Validation failed: {Errors}", ex.Errors);

      await WriteValidationProblemAsync(context, ex);
    }
    catch (DomainException ex)
    {
      _logger.LogWarning("Domain exception: {Message}", ex.Message);

      await WriteProblemAsync(
        context,
        StatusCodes.Status400BadRequest,
        "Domain Rule Violation",
        ex.Message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception");

      await WriteProblemAsync(
        context,
        StatusCodes.Status500InternalServerError,
        "Internal Server Error",
        "An unexpected error occurred. Please try again later.");
    }
  }

  private static Task WriteValidationProblemAsync(
    HttpContext context,
    ValidationException ex)
  {
    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    context.Response.ContentType = "application/problem+json";

    var errors = ex.Errors
      .GroupBy(e => e.PropertyName)
      .ToDictionary(
        g => g.Key,
        g => g.Select(e => e.ErrorMessage).ToArray());

    var problem = new HttpValidationProblemDetails
    {
      Errors = errors,
      Status = StatusCodes.Status400BadRequest,
      Title = "Validation Failed",
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    };

    return context.Response.WriteAsJsonAsync(problem);
  }

  private static Task WriteProblemAsync(
    HttpContext context,
    int statusCode,
    string title,
    string detail)
  {
    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/problem+json";

    var problem = new ProblemDetails
    {
      Status = statusCode, Title = title, Detail = detail, Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    };

    return context.Response.WriteAsJsonAsync(problem);
  }
}
