namespace LiskovSamples.InvoiceWorkflow;

// Invoice supertype
public class Invoice
{
    public string InvoiceNumber { get; protected set; }
    public decimal TotalAmount { get; protected set; }
    public DateTime InvoiceDate { get; protected set; }
    public InvoiceState State { get; protected set; }

    public Invoice(string invoiceNumber, decimal totalAmount, DateTime invoiceDate)
    {
        InvoiceNumber = invoiceNumber;
        TotalAmount = totalAmount;
        InvoiceDate = invoiceDate;
        State = InvoiceState.Draft;
    }
    
    public virtual bool SubmitForApproval()
    {
        if (TotalAmount <= 0)
            return false;

        if (State != InvoiceState.Draft)
            return false;

        State = InvoiceState.ReadyForApproval;
        return true;
    }
    
    public virtual bool Approve()
    {
        if (State != InvoiceState.ReadyForApproval)
            return false;

        State = InvoiceState.Approved;
        return true;
    }
    
    public virtual DateTime CalculateDueDate()
    {
        // Standard payment terms: 14 days net from invoice date
        return InvoiceDate.AddDays(14);
    }
    
    public virtual DateTime CalculateDiscountDate()
    {
        // 2/10 net 14: 2% discount if paid within 10 days from invoice date
        return InvoiceDate.AddDays(10);
    }
    
    public virtual decimal CalculateDiscountedAmount(DateTime paymentDate)
    {
        if (paymentDate <= CalculateDiscountDate())
        {
            // 2% discount if paid within discount period
            return TotalAmount * 0.98m;
        }
        return TotalAmount;
    }
}