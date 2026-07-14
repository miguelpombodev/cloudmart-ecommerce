namespace Identity.Application.Abstractions.Options;

public sealed class ExternalProvidersOptions
{
  public ExternalProvider Google { get; init; } = new();
}
