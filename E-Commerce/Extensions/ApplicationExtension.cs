using E_Commerce.Application.Common.Resolvers;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Application.Interfaces.Common;
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Application.Services.Authentication;
using E_Commerce.Application.Services.Common;
using E_Commerce.Application.Services.Core;
using E_Commerce.Core.Shared.Settings;
using E_Commerce.Core.Shared.Utilties.Identity;
using E_Commerce.Infrastructure.Persistence.Seeding.ExcelParser;
using E_Commerce.Repository.Repositories;
using E_Commerce.Repository.Repositories.Interfaces;

namespace Extensions;
public static class ApplicationExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IImportProductData, ImportProductData>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<JwtConfig>();
        services.AddScoped<GoogleData>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtAuthService, JwtAuthService>();
        services.AddScoped<EmailTemplateResolver>();
        services.AddScoped<TwoFactorTemplateResolver>();
        services.AddScoped<InvoiceTemplateResolver>();
        services.AddScoped<EmailCodes>();
        services.AddScoped<ChangeLogRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IChangeLogService, ChangeLogService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}

