using E_Commerce.Application.Interfaces.Common;
using System.Text.Json;
namespace E_Commerce.Application.Services.Common;
    public class TranslationService(IHostEnvironment _env, IMemoryCache _cache) : ITranslationService
    {

        public string GetTranslation(string key, string culture)
        { 
            var translations = GetAllTranslations(culture);

            if (translations.TryGetValue(key.ToLower(), out var exactMatch))
                return exactMatch;

            var words = key.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();

            int i = 0;
            while (i < words.Length)
            {
                bool found = false;
                for (int len = words.Length - i; len > 0; len--)
                {
                    var part = string.Join(' ', words.Skip(i).Take(len));

                    if (translations.TryGetValue(part, out var value))
                    {
                        result.Add(value);
                        i += len;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    result.Add(words[i]);
                    i++;
                }
            }

            return string.Join(' ', result);
        }



        public Dictionary<string, string> GetAllTranslations(string culture)
        {
            return _cache.GetOrCreate($"translations_{culture}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var filePath = Path.Combine(_env.ContentRootPath, "Resources", $"{culture}.json");
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<Dictionary<string, string>>(json, options)
                       ?? new Dictionary<string, string>();
            });
        }
    }


