
using E_Commerce.Application.Interfaces.Common;

namespace E_Commerce.Middlewares;
public class LocalizationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context, ITranslationService translationService)
    {
        var culture = context.Request.Headers["Accept-Language"]
                  .ToString()
                  .Split(',')[0];
        context.Items["Translations"] = translationService.GetAllTranslations(culture);
        await _next(context);
    }
}
 