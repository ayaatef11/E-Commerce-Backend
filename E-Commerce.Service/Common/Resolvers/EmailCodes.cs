//namespace E_Commerce.Application.Common.EmailMapping;
//    public class EmailCodes(IEmailSenderService _emailService,EmailTemplateResolver _emailResolver)
//    {
//        public async Task SendEmailConfirmationCode(string email, string confirmationCode, string fullname)
//        {
//            string title = "Confirm your email";


//            string message = await _emailResolver.ResolveCodesEmailAsync(confirmationCode, fullname, title, 2025);

//            await _emailService.SendEmailAsync(email, title, message);
//        }
//    }

using E_Commerce.Application.Interfaces.Authentication;

namespace E_Commerce.Application.Common.Resolvers;

public class EmailCodes(IEmailSenderService _emailService, ILogger<EmailCodes> _logger)
{
    public async Task<bool> SendEmailConfirmationCode(string email, string confirmationCode, string fullName)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(confirmationCode))
            {
                _logger.LogWarning("Email or confirmation code is null/empty");
                return false;
            }

            string title = "Confirm your email";
        string message = BuildEmailTemplate(title, fullName, confirmationCode, DateTime.Now.Year);

        bool result = await _emailService.SendEmailAsync(email, title, message);

        if (result)
        {
            _logger.LogInformation("Email confirmation sent successfully to {Email}", email);
        }
        else
        {
            _logger.LogWarning("Failed to send email confirmation to {Email}", email);
        }

        return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation to {Email}", email);
            return false;
        }
    }




    private string BuildEmailTemplate(string title, string fullName, string code, int year)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verification</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 10px;
            text-align: center;
            border-top-left-radius: 8px;
            border-top-right-radius: 8px;
        }}
        .content {{
            padding: 20px;
        }}
        .code {{
            font-size: 24px;
            font-weight: bold;
            text-align: center;
            margin-top: 20px;
            margin-bottom: 30px;
            color: #007bff;
        }}
        .footer {{
            background-color: #f7f7f7;
            padding: 10px;
            text-align: center;
            border-top: 1px solid #dddddd;
            font-size: 12px;
            color: #777777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>{title}</h2>
        </div>
        <div class='content'>
            <p>Dear {fullName},</p>
            <p>Please use the following verification code:</p>
            <div class='code'>{code}</div>
            <p>This code will expire in 24 hours. Please use it promptly to verify your email address.</p>
            <p>If you did not request this verification, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; {year} Causmatic Store. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}