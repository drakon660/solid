namespace LiskovSamples;

/// <summary>
/// Purchase Order - represents pre-approved spending authorization
/// In real systems, POs are created by purchasing department with approved budgets
/// </summary>
public class PurchaseOrder
{
    public string Number { get; }
    public decimal TotalAmount { get; }
    public string Vendor { get; }
    public DateTime ApprovedDate { get; }

    public PurchaseOrder(string number, decimal totalAmount, string vendor, DateTime approvedDate)
    {
        Number = number;
        TotalAmount = totalAmount;
        Vendor = vendor;
        ApprovedDate = approvedDate;
    }
}