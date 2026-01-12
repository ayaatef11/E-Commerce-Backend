namespace E_Commerce.Core.Shared.Settings;
public sealed record GoogleUserInformation
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
}

