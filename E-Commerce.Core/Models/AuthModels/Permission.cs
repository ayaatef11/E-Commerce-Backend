namespace E_Commerce.Core.Models.AuthModels;
    public class Permission
    {
        [Key]
        public string Name { get; set; }=string.Empty;
        public ICollection<RolePermission> RolePermissions { get; set; }
    }

