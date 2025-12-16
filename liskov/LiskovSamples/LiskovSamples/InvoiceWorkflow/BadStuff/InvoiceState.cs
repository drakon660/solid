namespace LiskovSamples;

/// <summary>
/// Invoice workflow states
/// </summary>
public enum InvoiceState
{
    Draft,
    ReadyForApproval,
    Approved,
    Rejected
}