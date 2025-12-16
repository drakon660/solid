namespace LiskovSamples.InvoiceWorkflow;

public class InvoiceWithPO : Invoice
{
    public PurchaseOrder PurchaseOrder { get; private set; }

    public InvoiceWithPO(string invoiceNumber, decimal amount, DateTime invoiceDate)
        : base(invoiceNumber, amount, invoiceDate)
    {
    }

    public void SetPurchaseOrder(PurchaseOrder purchaseOrder) => PurchaseOrder = purchaseOrder;

    /// <summary>
    /// VIOLATION: Throws PurchaseOrderMismatchException or PurchaseOrderNotSetException which clients don't expect
    /// Base class contract doesn't specify this exception
    /// Strengthens precondition: requires invoice amount to match PO total exactly
    /// </summary>
    public override bool Approve()
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
        // Business rule: Invoice amount must match PO total exactly
        if (TotalAmount != PurchaseOrder.TotalAmount)
        {
            throw new PurchaseOrderMismatchException(PurchaseOrder, TotalAmount);
        }

        return base.Approve();
    }

    /// <summary>
    /// VIOLATION: Uses PO approval date instead of invoice date as base
    /// Client expects due date calculated from invoice date
    /// This produces WRONG due dates breaking client expectations
    /// </summary>
    public override DateTime CalculateDueDate()
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
        // VIOLATION: Uses PO approval date + PO payment terms (60 days)
        // Client expects: InvoiceDate + 30 days
        // Gets: PO.ApprovedDate + 60 days
        return PurchaseOrder.ApprovedDate.AddDays(60);
    }

    /// <summary>
    /// VIOLATION: Uses PO approval date for discount calculation
    /// Client expects discount date from invoice date
    /// This produces WRONG discount deadlines
    /// </summary>
    public override DateTime CalculateDiscountDate()
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
        // VIOLATION: PO has pre-negotiated terms: pay within 15 days of PO approval for 3% discount
        // Client expects: InvoiceDate + 10 days for 2% discount
        // Gets: PO.ApprovedDate + 15 days for 3% discount
        return PurchaseOrder.ApprovedDate.AddDays(15);
    }

    /// <summary>
    /// VIOLATION: Applies different discount percentage (3%) and uses PO approval date
    /// Client expects: 2% discount if paid within 10 days of invoice date
    /// Gets: 3% discount if paid within 15 days of PO approval date
    /// This produces WRONG discounted amounts
    /// </summary>
    public override decimal CalculateDiscountedAmount(DateTime paymentDate)
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }

        // VIOLATION: PO terms - 3% discount if paid within 15 days of PO approval
        // Client expects: Amount * 0.98 (2% discount) based on invoice date
        // Gets: Amount * 0.97 (3% discount) based on PO approval date
        if (paymentDate <= CalculateDiscountDate())
        {
            return TotalAmount * 0.97m; // 3% discount (not 2%!)
        }
        return TotalAmount;
    }
}