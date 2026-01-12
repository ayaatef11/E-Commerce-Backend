using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.Common.DTOS.Requests;
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }=string.Empty;

        [Required]
        public string FullName { get; set; }=string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }= string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsTwoFactorEnabled { get; set; }
    }


