namespace E_Commerce.Core.Shared.Settings;

public sealed record GoogleData
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
}
