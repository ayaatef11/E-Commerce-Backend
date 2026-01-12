namespace E_Commerce.Extensions.MulitLanguage;
public static class RequestExtension
{

        public static string GetCultureFromRequest(this HttpRequest request)
        {
            var path = request.Path.Value;
            if (!string.IsNullOrEmpty(path))
            {
                var segments = path.Split('/');
                if (segments.Length > 1 && IsValidCulture(segments[1]))
                {
                    return segments[1];
                }
            }
            if (request.Query.TryGetValue("culture", out StringValues queryCulture) &&
                IsValidCulture(queryCulture))
            {
                return queryCulture;
            }
            if (request.Cookies.TryGetValue("culture", out string cookieCulture) &&
                IsValidCulture(cookieCulture))
            {
                return cookieCulture;
            }

            var acceptLanguage = request.GetTypedHeaders().AcceptLanguage;
            if (acceptLanguage?.Count > 0)
            {
                foreach (var language in acceptLanguage.OrderByDescending(l => l.Quality ?? 1))
                {
                    var culture = language.Value.ToString();
                    if (IsValidCulture(culture))
                    {
                        return culture;
                    }
                }
            }
            return "en-US"; 
        }

        private static bool IsValidCulture(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                return false;

            try
            {
                var culture = new CultureInfo(cultureName);
                return true;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }
    }





