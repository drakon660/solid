using LiskovSamples.InvoiceWorkflow;
using LiskovSamples.InvoiceWorkflow.GoodStuff;

namespace LiskovSamples;

public class InvoiceService
{
    public bool ApproveInvoice(Invoice invoice)
    {
        try
        {
            // First submit, then approve
            if (!invoice.SubmitForApproval())
                return false;

            return invoice.Approve();
        }
        catch (ArgumentException)
        {
            // Client only expects standard exceptions based on base class contract
            return false;
        }
        // PurchaseOrderMismatchException will crash the application!
    }

    public DateTime GetDueDate(Invoice invoice) => invoice.CalculateDueDate();

    public int ApproveInvoiceBatch(List<Invoice> invoices)
    {
        int approved = 0;
        foreach (var invoice in invoices)
        {
            if (invoice.SubmitForApproval() && invoice.Approve()) // May throw unexpected exceptions
            {
                approved++;
            }
        }

        return approved;
    }

    public static InvoiceResult ApproveInvoiceCorrect(IInvoiceDocument invoice)
    {
        var submitResult = invoice.SubmitForApproval();
        if (!submitResult.Success)
            return submitResult;

        return invoice.Approve();
    }
    
    public static Dictionary<string, List<string>> ApproveInvoiceBatchCorrect(List<IInvoiceDocument> invoices)
    {
        var results = new Dictionary<string, List<string>>
        {
            ["Successful"] = new List<string>(),
            ["Failed"] = new List<string>()
        };

        foreach (var invoice in invoices)
        {
            var submitResult = invoice.SubmitForApproval();
            if (!submitResult.Success)
            {
                results["Failed"].Add($"{invoice.InvoiceNumber}: {submitResult.ErrorCode} - {submitResult.ErrorMessage}");
                continue;
            }

            var approveResult = invoice.Approve();
            if (approveResult.Success)
            {
                results["Successful"].Add(invoice.InvoiceNumber);
            }
            else
            {
                results["Failed"].Add($"{invoice.InvoiceNumber}: {approveResult.ErrorCode} - {approveResult.ErrorMessage}");
            }
        }

        return results;
    }
}