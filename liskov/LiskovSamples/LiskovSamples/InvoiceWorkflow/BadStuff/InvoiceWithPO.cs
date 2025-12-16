namespace LiskovSamples.InvoiceWorkflow;

public class InvoiceWithPO : Invoice
{
    public PurchaseOrder PurchaseOrder { get; private set; }

    public InvoiceWithPO(string invoiceNumber, decimal amount, DateTime invoiceDate)
        : base(invoiceNumber, amount, invoiceDate)
    {
    }

    public void SetPurchaseOrder(PurchaseOrder purchaseOrder) => PurchaseOrder = purchaseOrder;
    
    public override bool Approve()
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
      
        if (TotalAmount != PurchaseOrder.TotalAmount)
        {
            throw new PurchaseOrderMismatchException(PurchaseOrder, TotalAmount);
        }

        return base.Approve();
    }
    
    public override DateTime CalculateDueDate()
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
        
        return PurchaseOrder.ApprovedDate.AddDays(60);
    }
    
    public override DateTime CalculateDiscountDate()
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
       
        return PurchaseOrder.ApprovedDate.AddDays(15);
    }
    
    public override decimal CalculateDiscountedAmount(DateTime paymentDate)
    {
        if (PurchaseOrder is null)
        {
            throw new PurchaseOrderNotSetException();
        }
        
        if (paymentDate <= CalculateDiscountDate())
        {
            return TotalAmount * 0.97m; // 3% discount (not 2%!)
        }
        return TotalAmount;
    }
}