namespace E_Commerce.Core.Shared.Utilties.Identity;

public class IdempotencyEntry
{
    public string RequestHash { get; set; } = null!;
    public int StatusCode { get; set; }
    public string ResponseBody { get; set; } = null!;
}