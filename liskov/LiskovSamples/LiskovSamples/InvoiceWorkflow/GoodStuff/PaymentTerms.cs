namespace LiskovSamples.InvoiceWorkflow.GoodStuff;

public readonly record struct PaymentTerms(
    DateTime DueDate,
    DateTime DiscountDate,
    decimal DiscountPercentage,
    int NetDays,
    int DiscountDays);