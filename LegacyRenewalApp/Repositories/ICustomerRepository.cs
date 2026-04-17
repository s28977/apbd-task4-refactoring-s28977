namespace LegacyRenewalApp;

public interface ICustomerRepository
{
    public Customer GetById(int customerId);
}