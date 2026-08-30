namespace BuildingBlocks.Options;


public sealed class KeyValueOptions
{
  public string Key { get; init; } = string.Empty;

  public object Value { get; init; } = new();
}

public sealed class OpenTelemetryOptions
{
  public string ApplicationName { get; init; } = string.Empty;
  public string ServiceName { get; init; } = string.Empty;

  public string OtelUrl { get; init; } = string.Empty;

  public IEnumerable<KeyValueOptions> OtelAttributes { get; init; } = [];
}
