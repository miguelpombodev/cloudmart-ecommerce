using System.Text.Json;
using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Outbox;

public sealed class OutboxMessage
{
  private OutboxMessage()
  {
  }

  private OutboxMessage(
    Guid id,
    string type,
    string payload,
    DateTimeOffset occuredOn)
  {
    Id = id;
    Type = type;
    Payload = payload;
    OccuredOn = occuredOn;
  }

  public Guid Id { get; private set; }

  public string Type { get; private set; } = default!;

  public string Payload { get; private set; } = default!;

  public DateTimeOffset OccuredOn { get; private set; }

  public DateTimeOffset? ProcessedOn { get; private set; }

  public string? Error { get; private set; }

  public bool IsProcessed => ProcessedOn.HasValue;

  public static OutboxMessage CreateFromEvent(IDomainEvent domainEvent)
  {
    string payload = JsonSerializer.Serialize(
      domainEvent,
      domainEvent.GetType(),
      JsonSerializerOptions.Default);

    return new OutboxMessage(
      domainEvent.EventId,
      domainEvent.EventType!,
      payload,
      domainEvent.OccuredOn
    );
  }

  public void MarkAsProcessed() =>
    ProcessedOn = DateTimeOffset.UtcNow;

  public void MarkAsFailed(string error)
  {
    Error = error;
    ProcessedOn = DateTimeOffset.UtcNow;
  }
}
