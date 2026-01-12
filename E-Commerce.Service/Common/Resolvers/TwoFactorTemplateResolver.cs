using E_Commerce.Core.Shared.Utilties;

namespace E_Commerce.Application.Common.Resolvers;
    public class TwoFactorTemplateResolver
    {
        public async Task<string> ResolveCodesTwoFactorAsync(string token, string FullName)
        {
            string templatePath = PATH.TwoFactorPath;
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Two Factor  template not found.", templatePath);
            }

            string messageTemplate = await File.ReadAllTextAsync(templatePath);
        string messageBody = messageTemplate
            .Replace("{Full_Name}", FullName)
            .Replace("{token}", token);
            return messageBody;
        }
    }

