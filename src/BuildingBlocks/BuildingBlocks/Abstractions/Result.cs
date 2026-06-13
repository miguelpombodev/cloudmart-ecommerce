namespace BuildingBlocks.Abstractions;

public sealed class Result<T>
{
  private readonly Error? _error;

  private readonly T? _value;


  private Result(T value)
  {
    IsSuccess = true;
    _value = value;
    _error = default;
  }

  private Result(Error error)
  {
    IsSuccess = false;
    _value = default;
    _error = error;
  }

  public bool IsSuccess { get; }

  public bool IsFailure => !IsSuccess;

  public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value of a failed Result");

  public Error Error =>
    IsFailure ? _error! : throw new InvalidOperationException("Cannot access Error of a successful Result.");

  public static Result<T> Success(T value) =>
    new(value);

  public static Result<T> Failure(Error error) =>
    new(error);

  public static implicit operator Result<T>(T value) =>
    Success(value);

  public static implicit operator Result<T>(Error error) =>
    Failure(error);

  public TResult Match<TResult>(
    Func<T, TResult> onSuccess,
    Func<Error, TResult> onFailure) =>
    IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}
