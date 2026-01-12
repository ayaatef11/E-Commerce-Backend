//using System.Globalization;

//namespace E_Commerce.Application.Common.Resolvers;
//    public class InvoiceTemplateResolver
//    {
//    public async Task<string> ResolveInvoiceTemplate(string FullName, Invoice invoice)
//    {
//        string templatePath = PATH.InvoicePath;
//        if (!File.Exists(templatePath))
//        {
//            throw new FileNotFoundException("Invoice template not found.", templatePath);
//        }

//        string messageTemplate = await File.ReadAllTextAsync(templatePath);
//        string messageBody = messageTemplate
//      .Replace("{FullName}", FullName)
//      .Replace("{invoice.Id}", invoice.Id.ToString())          
//      .Replace("{invoice.TotalAmount}", invoice.TotalAmount.ToString(/*"C", CultureInfo.CreateSpecificCulture("en-EG")*/))  
//      .Replace("{invoice.OrderDate}", invoice.OrderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));  
//        return messageBody;
//    }
//}

namespace E_Commerce.Application.Common.Resolvers;

public class InvoiceTemplateResolver
{
    public Task<string> ResolveInvoiceTemplate(string fullName, Invoice invoice)
    {
        string messageBody = BuildInvoiceHtml(fullName, invoice);
        return Task.FromResult(messageBody);
    }

    private string BuildInvoiceHtml(string fullName, Invoice invoice)
    {
        string htmlTemplate = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Causmatic Store Invoice</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
        }
        .container {
            max-width: 600px;
            margin: auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }
        .header {
            background-color: #007bff;
            color: #ffffff;
            padding: 10px;
            text-align: center;
            border-top-left-radius: 8px;
            border-top-right-radius: 8px;
        }
        .content {
            padding: 20px;
        }
        .invoice-details {
            margin: 20px 0;
            padding: 15px;
            background-color: #f9f9f9;
            border-radius: 5px;
        }
        .footer {
            background-color: #f7f7f7;
            padding: 10px;
            text-align: center;
            border-top: 1px solid #dddddd;
            font-size: 12px;
            color: #777777;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Invoice</h2>
        </div>
        <div class='content'>
            <p>Dear {FullName},</p>
            <p>Here is your invoice details:</p>
            <div class='invoice-details'>
                <h3>Invoice Number: {InvoiceId}</h3>
                <p><strong>Total Cost:</strong> {TotalAmount} EGP</p>
                <p><strong>Creation Date:</strong> {OrderDate}</p>
            </div>
        </div>
        <div class='footer'>
            <p>Thank you for using our service.</p>
            <p>&copy; {CurrentYear} Causmatic Store. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return htmlTemplate
            .Replace("{FullName}", fullName)
            .Replace("{InvoiceId}", invoice.Id.ToString())
            .Replace("{TotalAmount}", invoice.TotalAmount.ToString("N2", new CultureInfo("en-EG")))
            .Replace("{OrderDate}", invoice.OrderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Replace("{CurrentYear}", DateTime.Now.Year.ToString());
    }
}