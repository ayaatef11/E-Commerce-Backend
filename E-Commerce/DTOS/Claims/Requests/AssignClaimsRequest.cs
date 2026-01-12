namespace E_Commerce.DTOS.Claims.Requests;
    public class AssignClaimsRequest
    {
    public string UserId { get; set; } = null!;
    public string ClaimType { get; set; } = "Permission"; // Default
    public List<string> ClaimValues { get; set; } = new();
}

