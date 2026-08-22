using Cloudmart.Contracts.Messaging.Enums;
using Cloudmart.Contracts.Messaging.Interfaces.Notifications;

namespace BuildingBlocks.Events.Notifications;

public record NotificationRequested: INotificationRequest
{
  public Guid Id { get; set; }

  public NotificationChannel Channel { get; } = NotificationChannel.Email;

  public string Recipient { get; init; } = string.Empty;

  public string Project { get; init; } = "cloudmart";

  public string Template { get; init; } = string.Empty;

  public Dictionary<string, object> Data { get; init; } = new();
};

