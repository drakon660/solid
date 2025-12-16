namespace LiskovSamples.InvoiceWorkflow.GoodStuff;

public class PurchaseOrderInvoiceCompliant : IInvoiceDocument
{
    public string InvoiceNumber { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public InvoiceState State { get; private set; }
    public PurchaseOrder PurchaseOrder { get; private set; }

    public PurchaseOrderInvoiceCompliant(string invoiceNumber, decimal amount, DateTime invoiceDate,
        PurchaseOrder purchaseOrder)
    {
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        InvoiceDate = invoiceDate;
        State = InvoiceState.Draft;
        PurchaseOrder = purchaseOrder;
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
        
        if (Amount != PurchaseOrder.TotalAmount)
        {
            var details = new Dictionary<string, object>
            {
                ["PurchaseOrderNumber"] = PurchaseOrder.Number,
                ["PurchaseOrderVendor"] = PurchaseOrder.Vendor,
                ["PurchaseOrderTotal"] = PurchaseOrder.TotalAmount,
                ["InvoiceAmount"] = Amount,
                ["Difference"] = Amount - PurchaseOrder.TotalAmount
            };
            return InvoiceResult.Failed("PO_AMOUNT_MISMATCH",
                $"Invoice amount {Amount:C} does not match PO {PurchaseOrder.Number} total {PurchaseOrder.TotalAmount:C}",
                details);
        }

        State = InvoiceState.Approved;
        return InvoiceResult.Successful();
    }
    
    public PaymentTerms GetPaymentTerms()
    {
        // PO has pre-negotiated terms: 3/15 net 60 from PO approval date
        // Different from standard invoice, but EXPLICIT - not hidden in calculation
        return new PaymentTerms(
            PurchaseOrder.ApprovedDate.AddDays(60),
            PurchaseOrder.ApprovedDate.AddDays(15),
            3.0m,
            60,
            15);
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