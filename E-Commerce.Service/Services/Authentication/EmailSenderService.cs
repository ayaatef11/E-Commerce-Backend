using E_Commerce.Application.Interfaces.Authentication;
using MailKit.Security;
using System.Text.RegularExpressions;

namespace E_Commerce.Application.Services.Authentication;

public class EmailSenderService(IConfiguration _configuration,ILogger<EmailSenderService> _logger) : IEmailSenderService
{

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, byte[] fileBytes = null, string fileName = null)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["Email:SenderName"], _configuration["Email:SenderEmail"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            if (fileBytes != null && !string.IsNullOrEmpty(fileName)) bodyBuilder.Attachments.Add(fileName, fileBytes, ContentType.Parse("application/pdf"));

            message.Body = bodyBuilder.ToMessageBody();
            var smtpPort = int.Parse(_configuration["Email:Port"] ?? "587");

            using var client = new SmtpClient();
            // Set timeout for production environments
            client.Timeout = 30000; // 30 seconds
                                    // For HostMonster: Use SSL on Connect for port 465, or StartTls for port 587
            var port = smtpPort;
            SecureSocketOptions sslOptions;

            if (port == 465)
            {
                sslOptions = SecureSocketOptions.SslOnConnect; // SSL from start
            }
            else if (port == 587)
            {
                sslOptions = SecureSocketOptions.StartTls; // STARTTLS
            }
            else if (port == 26)
            {
                sslOptions = SecureSocketOptions.None; // No SSL (HostMonster alternative port)
            }
            else
            {
                sslOptions = SecureSocketOptions.Auto; // Auto-detect
            }

            await client.ConnectAsync(
                _configuration["Email:Host"],
                int.Parse(_configuration["Email:Port"]),
           sslOptions);
          

            if (!client.IsConnected)
            {
                throw new InvalidOperationException("Failed to connect to the SMTP server.");
            }
            await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);

            if (!client.IsAuthenticated)
            {
                _logger.LogError("Failed to authenticate with SMTP server");
                return false;
            }
            var response = await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner error: {ex.InnerException.Message}");

            throw new Exception("Failed to send email", ex);
        }
    }


public  bool ValidateEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return false;
    string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";  

    try
    {
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }
    catch (RegexMatchTimeoutException)
    {
        return false;
    }
}

}


