namespace Api.Extensions;

public static class CorsPolicyExtensions
{
    public static IServiceCollection ConfigureCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        return services;
    }
}