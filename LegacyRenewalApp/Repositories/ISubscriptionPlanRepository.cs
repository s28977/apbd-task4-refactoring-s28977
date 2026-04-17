namespace LegacyRenewalApp;

public interface ISubscriptionPlanRepository
{
    public SubscriptionPlan GetByCode(string code);
}