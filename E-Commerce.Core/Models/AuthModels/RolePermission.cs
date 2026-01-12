namespace E_Commerce.Core.Models.AuthModels;
[PrimaryKey(nameof(RoleId), nameof(PermissionName))]

public class RolePermission
{
    public string RoleId { get; set; } = string.Empty;

    public string PermissionName { get; set; }=string.Empty;
    public IdentityRole? Role { get; set; }
    public Permission? Permission { get; set; }
}
