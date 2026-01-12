namespace E_Commerce.Application.Interfaces.Common
{
    public interface ITranslationService
    {
        string GetTranslation(string key, string culture);
        Dictionary<string, string> GetAllTranslations(string culture);
    }
}
