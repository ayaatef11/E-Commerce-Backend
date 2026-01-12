using E_Commerce.Core.Shared.Utilties;

namespace E_Commerce.Application.Common.Resolvers;
    public class EmailTemplateResolver
    {    
        public async Task<string> ResolveCodesEmailAsync(string ConfirmationCode, string FullName, string title, int year)
        {
        string templatePath = PATH.HTMLpath;
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Email template not found.", templatePath);
            }

            string messageTemplate = await File.ReadAllTextAsync(templatePath);
            string messageBody = messageTemplate
                .Replace("{FullName}", FullName)
                .Replace("{Code}", ConfirmationCode)
                .Replace("Title", title)
                .Replace("Year", year.ToString());
            return messageBody;
        }
    }

