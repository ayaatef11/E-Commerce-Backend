namespace E_Commerce.DTOS.User.Request
{
    public class UserRequest
    {
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public IList<string> Roles { get; set; }    
    }
}
