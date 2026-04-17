namespace LegacyRenewalApp;

public class LegacyBillingGatewayInvoiceSaverWrapper : IInvoiceSaver
{
    public void SaveInvoice(RenewalInvoice invoice)
    {
        LegacyBillingGateway.SaveInvoice(invoice);
    }
}