namespace LiskovSamples.InvoiceWorkflow.GoodStuff;

public class InvoiceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public Dictionary<string, object>? ErrorDetails { get; init; }

    public static InvoiceResult Successful() => new() { Success = true };

    public static InvoiceResult Failed(string errorCode, string errorMessage,
        Dictionary<string, object>? details = null) =>
        new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage, ErrorDetails = details };
}