using CSharpFunctionalExtensions;

namespace LiskovSamples.InvoiceWorkflow.DddStuff;

public sealed class Invoice : Entity<int>
{
    public string Number { get; }
    public decimal Amount { get; private set; }
    public DateTime IssuedOn { get; }
    public InvoiceState State { get; private set; }
    public IReadOnlyCollection<PaymentTerms> PaymentTerms { get; private set; }

    private Invoice(string number, decimal amount, DateTime issuedOn)
    {
        Number = string.IsNullOrWhiteSpace(number)
            ? throw new ArgumentException("Invoice number must be provided", nameof(number))
            : number;

        Amount = amount;
        IssuedOn = issuedOn;
        State = InvoiceState.Draft;
        PaymentTerms = Array.AsReadOnly(new[] { DddStuff.PaymentTerms.CreateStandard() });
    }
    
    public static Invoice Create(string number, decimal amount, DateTime issuedOn)
        => new Invoice(number, amount, issuedOn);
    
    public bool UpdateAmount(decimal newAmount)
    {
        if (State != InvoiceState.Draft)
            return false;

        Amount = newAmount;
        return true;
    }
    
    public bool SubmitForApproval()
    {
        if (Amount <= 0)
            return false;
        if (State != InvoiceState.Draft)
            return false;

        State = InvoiceState.ReadyForApproval;
        return true;
    }
    
    public bool Approve()
    {
        if (State != InvoiceState.ReadyForApproval)
            return false;

        State = InvoiceState.Approved;
        return true;
    }

   
    public DateTime DueDate()
    {
        var terms = GetTerms();
        return IssuedOn.AddDays(terms.DaysToPay);
    }
    
    public DateTime DiscountDate()
    {
        var terms = GetTerms();
        return IssuedOn.AddDays(terms.DaysToPay);
    }
    
    public decimal AmountIfPaidOn(DateTime paymentDate)
    {
        var terms = GetTerms(paymentDate);
        var discountDeadline = IssuedOn.AddDays(terms.DaysToPay);
        return paymentDate <= discountDeadline
            ? Amount * (1 - terms.DiscountPercentage / 100m)
            : Amount;
    }

    private PaymentTerms GetTerms()
    {
        if (PaymentTerms is { Count: > 0 })
        {
            foreach (var t in PaymentTerms)
                return t;
        }
        return DddStuff.PaymentTerms.CreateStandard();
    }
    
    private PaymentTerms GetTerms(DateTime asOfDate)
    {
        return GetTerms();
    }
}
