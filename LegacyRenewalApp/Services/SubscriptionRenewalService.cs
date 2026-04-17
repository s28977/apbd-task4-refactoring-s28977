using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService(
        ICustomerRepository customerRepository,
        ISubscriptionPlanRepository planRepository)
    {
        private ICustomerRepository  _customerRepository = customerRepository;
        private ISubscriptionPlanRepository _planRepository = planRepository;

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            //Validate 
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            //Normalize
            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            
            //Communicate with database
            var customer = customerRepository.GetById(customerId);
            var plan = planRepository.GetByCode(normalizedPlanCode);
            
            // Validate
            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            PaymentCalculator paymentCalculator = new PaymentCalculator();

            decimal baseAmount = paymentCalculator.CalculateBaseAmount(plan, seatCount);
            decimal discountAmount = paymentCalculator.CalculateDiscount(baseAmount, plan, customer, seatCount, useLoyaltyPoints);
            decimal subtotalAfterDiscount = paymentCalculator.CalculateSubtotal(baseAmount, discountAmount);
            decimal supportFee = paymentCalculator.CalculateSupportFee(includePremiumSupport, normalizedPlanCode);
            decimal paymentFee = paymentCalculator.CalculatePaymentFee(subtotalAfterDiscount, supportFee, normalizedPaymentMethod);
            decimal taxBase = paymentCalculator.CalculateTaxBase(subtotalAfterDiscount, supportFee, paymentFee);
            decimal taxAmount = paymentCalculator.CalculateTaxAmount(taxBase, customer);
            decimal finalAmount = paymentCalculator.CalculateFinalAmount(taxBase, taxAmount);
            string notes = paymentCalculator.Notes;

            // Generate Invoice
            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
            
            //Save invoice
            LegacyBillingGateway.SaveInvoice(invoice);
            
            //Validate email
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                //Send email
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                LegacyBillingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
