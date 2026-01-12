namespace E_Commerce.Core.Models.AuthModels;
public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime RevokedDate { get; set; }
    [ForeignKey(nameof(AppUserId))]
    public string AppUserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

}


