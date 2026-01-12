namespace E_Commerce.Core.Shared.Results;
public sealed record ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}