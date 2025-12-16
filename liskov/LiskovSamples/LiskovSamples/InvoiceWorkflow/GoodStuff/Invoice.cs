namespace LiskovSamples.InvoiceWorkflow.GoodStuff;

public class StandardInvoice : IInvoiceDocument
{
    public string InvoiceNumber { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public InvoiceState State { get; private set; }

    public StandardInvoice(string invoiceNumber, decimal amount, DateTime invoiceDate)
    {
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        InvoiceDate = invoiceDate;
        State = InvoiceState.Draft;
    }

    public InvoiceResult SubmitForApproval()
    {
        if (Amount <= 0)
            return InvoiceResult.Failed("INVALID_AMOUNT", "Invoice amount must be greater than zero");

        if (State != InvoiceState.Draft)
            return InvoiceResult.Failed("INVALID_STATE", $"Cannot submit invoice in {State} state");

        State = InvoiceState.ReadyForApproval;
        return InvoiceResult.Successful();
    }

    public InvoiceResult Approve()
    {
        if (State != InvoiceState.ReadyForApproval)
            return InvoiceResult.Failed("INVALID_STATE", $"Cannot approve invoice in {State} state");

        State = InvoiceState.Approved;
        return InvoiceResult.Successful();
    }

    public PaymentTerms GetPaymentTerms()
    {
        // Standard terms: 2/10 net 14 from invoice date
        return new PaymentTerms(
            InvoiceDate.AddDays(14),
            InvoiceDate.AddDays(10),
            2.0m,
            14,
            10);
    }

    public decimal CalculateDiscountedAmount(DateTime paymentDate)
    {
        var terms = GetPaymentTerms();
        if (paymentDate <= terms.DiscountDate)
        {
            return Amount * (1 - terms.DiscountPercentage / 100m);
        }
        return Amount;
    }
}