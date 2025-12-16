using CSharpFunctionalExtensions;

namespace LiskovSamples.InvoiceWorkflow.DddStuff;

public class PaymentTerms : ValueObject
{
    public decimal DiscountPercentage { get; protected set; }
    public int DaysToPay { get; protected set; }
    
    private PaymentTerms(decimal discountAmount, int days)
    {
        DiscountPercentage = discountAmount;
        DaysToPay = days;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DiscountPercentage;
        yield return DaysToPay;
    }

    public static PaymentTerms CreateStandard() => new (1, 14);
}