using Cloudmart.Contracts.Messaging.Interfaces.Notifications;

namespace BuildingBlocks.Events.Notifications.Factories.WelcomeEmail;

public static class WelcomeEmailNotification
{
	private const string TemplateName = "welcome";
	private const string Subject = "Welcome to CloudMart!";
	private const string LoginUrl = "https://cloudmart.example.com/login";

	public static INotificationRequest Create(WelcomeEmailData data)
	{

		return new NotificationRequested
		{
			Recipient = data.userEmailAdress,
			Template = TemplateName,
			Data =
			{
				["name"] = data.userName,
				["subject"] = Subject,
				["loginUrl"] = LoginUrl,
			}
		};
	}
}
