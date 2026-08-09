namespace Identity.Application.Abstractions.Options;

public sealed class ExternalProvider
{
  public string Url { get; init; } = string.Empty;

  public string ClientId { get; init; } = string.Empty;

  public string ClientSecret { get; init; } = string.Empty;

  public TimeSpan ResponseTimeout { get; init; } = TimeSpan.FromMinutes(1);
}
