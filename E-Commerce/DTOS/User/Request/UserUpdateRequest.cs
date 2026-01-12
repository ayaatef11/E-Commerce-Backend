namespace E_Commerce.DTOS.User.Request;
public class UserUpdateRequest
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; }= null!;
    public string Address { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
}
