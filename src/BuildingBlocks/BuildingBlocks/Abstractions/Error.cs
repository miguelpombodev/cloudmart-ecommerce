namespace BuildingBlocks.Abstractions;

public record Error(string InternalCode, string Description, int StatusCode = 500)
{
  public static readonly Error None = new(string.Empty, string.Empty);

  public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

  public static Error NotFound(string description) =>
    new("Error.NotFound", description, 404);

  public static Error Validation(string code, string description) =>
    new(code, description);

  public static Error Conflict(string description) =>
    new("Error.Conflict", description, 409);

  public static Error Unauthorized(string description) =>
    new("Error.Unauthorized", description, 401);

  public static Error Failure(string description) =>
    new("Error.OperationFailed", description);
}
