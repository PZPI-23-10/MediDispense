using Application.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUrlSafeEncoder, UrlSafeEncoder>();
        services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();

        return services;
    }
}