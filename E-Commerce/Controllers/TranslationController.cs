using E_Commerce.Application.Interfaces.Common;
using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/[controller]")]
    [ApiController]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]

public class TranslationController(ITranslationService _translationService) : ControllerBase
    {

        [HttpGet("{culture}")]
        public IActionResult GetAllTranslations(string culture)
        {
            var translations = _translationService.GetAllTranslations(culture);
            return Ok(translations);
        }

        [HttpGet("{culture}/{key}")]
        public IActionResult GetTranslation(string culture, string key)
        {
            var translation = _translationService.GetTranslation(key, culture);
            return Ok(new { Key = key, Value = translation });
        }
    }

