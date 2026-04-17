namespace LegacyRenewalApp;

public class LegacyBillingGatewayEmailSenderWrapper : INotificationSender
{
    public void SendNotification(string to, string subject, string body)
    {
        LegacyBillingGateway.SendEmail(to, subject, body);
    }
}