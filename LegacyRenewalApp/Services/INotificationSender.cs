namespace LegacyRenewalApp;

public interface INotificationSender
{
    public void SendNotification(string to, string subject, string body);
}