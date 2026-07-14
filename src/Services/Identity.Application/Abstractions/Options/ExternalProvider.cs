namespace Identity.Application.Abstractions.Options;

public sealed class ExternalProvider
{
  public string ClientId { get; init; } = string.Empty;
  public string ClientSecret { get; init; } = string.Empty;
}
