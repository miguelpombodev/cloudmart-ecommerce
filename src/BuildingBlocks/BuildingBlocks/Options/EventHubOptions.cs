namespace BuildingBlocks.Options;

public sealed class EventHubOptions
{
  public string Host { get; set; } = string.Empty;

  public string Username { get; set; } = string.Empty;

  public string Password { get; set; } = string.Empty;

  public string ProviderName { get; set; } = string.Empty;
}
