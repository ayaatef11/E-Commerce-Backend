namespace E_Commerce.Application.Interfaces.Authentication;
    public interface IEmailSenderService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, byte[] fileBytes = null, string fileName = null);
        bool ValidateEmail(string email);
    }

