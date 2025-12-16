namespace LiskovSamples.InvoiceWorkflow;

public class PurchaseOrderMismatchException : Exception
{
    public PurchaseOrder PurchaseOrder { get; }
    public decimal InvoiceAmount { get; }

    public PurchaseOrderMismatchException(PurchaseOrder purchaseOrder, decimal invoiceAmount)
        : base(
            $"Invoice amount {invoiceAmount:C} does not match PO {purchaseOrder.Number} total {purchaseOrder.TotalAmount:C}")
    {
        PurchaseOrder = purchaseOrder;
        InvoiceAmount = invoiceAmount;
    }
}

public class PurchaseOrderNotSetException : Exception
{
    
}