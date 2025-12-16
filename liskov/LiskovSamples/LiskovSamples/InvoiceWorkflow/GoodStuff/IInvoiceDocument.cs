namespace LiskovSamples.InvoiceWorkflow.GoodStuff;

public interface IInvoiceDocument
{
    string InvoiceNumber { get; }
    decimal Amount { get; }
    InvoiceState State { get; }

    InvoiceResult SubmitForApproval();
    InvoiceResult Approve();
    PaymentTerms GetPaymentTerms();
    decimal CalculateDiscountedAmount(DateTime paymentDate);
}