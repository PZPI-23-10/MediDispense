using System.Text.Json.Serialization;

namespace Api.Extensions;

public static class ControllersExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        return services;
    }
}