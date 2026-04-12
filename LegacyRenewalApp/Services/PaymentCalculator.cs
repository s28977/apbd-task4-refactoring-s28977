using System;

namespace LegacyRenewalApp;

public class PaymentCalculator
{
    public decimal CalculateBaseAmount(SubscriptionPlan plan, int seatCount)
    {
        return (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
    }

    public decimal CalculateDiscount(decimal baseAmount, SubscriptionPlan plan, Customer customer, int seatCount,
        bool useLoyaltyPoints)
    {
        decimal discountAmount = 0m;
        if (customer.Segment == "Silver")
        {
            discountAmount += baseAmount * 0.05m;
        }
        else if (customer.Segment == "Gold")
        {
            discountAmount += baseAmount * 0.10m;
        }
        else if (customer.Segment == "Platinum")
        {
            discountAmount += baseAmount * 0.15m;
        }
        else if (customer.Segment == "Education" && plan.IsEducationEligible)
        {
            discountAmount += baseAmount * 0.20m;
        }

        if (customer.YearsWithCompany >= 5)
        {
            discountAmount += baseAmount * 0.07m;
        }
        else if (customer.YearsWithCompany >= 2)
        {
            discountAmount += baseAmount * 0.03m;
        }

        if (seatCount >= 50)
        {
            discountAmount += baseAmount * 0.12m;
        }
        else if (seatCount >= 20)
        {
            discountAmount += baseAmount * 0.08m;
        }
        else if (seatCount >= 10)
        {
            discountAmount += baseAmount * 0.04m;
        }

        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
            discountAmount += pointsToUse;
        }

        return discountAmount;
    }

    public decimal CalculateSubtotal(decimal baseAmount, decimal discountAmount)
    {
        decimal subtotalAfterDiscount = baseAmount - discountAmount;
        if (subtotalAfterDiscount < 300m)
        {
            subtotalAfterDiscount = 300m;
        }

        return subtotalAfterDiscount;
    }

    public decimal CalculateSupportFee(bool includePremiumSupport, string normalizedPlanCode)
    {
        decimal supportFee = 0m;
        if (includePremiumSupport)
        {
            if (normalizedPlanCode == "START")
            {
                supportFee = 250m;
            }
            else if (normalizedPlanCode == "PRO")
            {
                supportFee = 400m;
            }
            else if (normalizedPlanCode == "ENTERPRISE")
            {
                supportFee = 700m;
            }
        }

        return supportFee;
    }

    public decimal CalculatePaymentFee(decimal subtotalAfterDiscount, decimal supportFee,
        string normalizedPaymentMethod)
    {
        decimal paymentFee = 0m;
        if (normalizedPaymentMethod == "CARD")
        {
            paymentFee = (subtotalAfterDiscount + supportFee) * 0.02m;
        }
        else if (normalizedPaymentMethod == "BANK_TRANSFER")
        {
            paymentFee = (subtotalAfterDiscount + supportFee) * 0.01m;
        }
        else if (normalizedPaymentMethod == "PAYPAL")
        {
            paymentFee = (subtotalAfterDiscount + supportFee) * 0.035m;
        }
        else if (normalizedPaymentMethod == "INVOICE")
        {
            paymentFee = 0m;
        }
        else
        {
            throw new ArgumentException("Unsupported payment method");
        }

        return paymentFee;
    }

    public decimal CalculateTaxBase(decimal subtotalAfterDiscount, decimal supportFee, decimal paymentFee)
    {
        return subtotalAfterDiscount + supportFee + paymentFee;
    }

    public decimal CalculateTaxAmount(decimal taxBase, Customer customer)
    {
        decimal taxRate = 0.20m;
        if (customer.Country == "Poland")
        {
            taxRate = 0.23m;
        }
        else if (customer.Country == "Germany")
        {
            taxRate = 0.19m;
        }
        else if (customer.Country == "Czech Republic")
        {
            taxRate = 0.21m;
        }
        else if (customer.Country == "Norway")
        {
            taxRate = 0.25m;
        }

        decimal taxAmount = taxBase * taxRate;

        return taxAmount;
    }


    public decimal CalculateFinalAmount(decimal taxBase, decimal taxRate, decimal taxAmount)
    {
        decimal finalAmount = taxBase + taxAmount;

        if (finalAmount < 500m)
        {
            finalAmount = 500m;
        }

        return finalAmount;
    }
}