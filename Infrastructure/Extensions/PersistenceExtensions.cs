using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

         services.AddDbContext<IDataContext, DataContext>(options => options.UseNpgsql(connectionString)
            .UseAsyncSeeding(DataSeeder.Seed)
            .UseSeeding((context, cancellationToken) =>
                DataSeeder.Seed(context, cancellationToken)
                    .GetAwaiter()
                    .GetResult()
            )
            .UseLazyLoadingProxies()
        );

        return services;
    }

    public static async Task<IServiceProvider> SeedIdentity(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Admin));

        if (!await roleManager.RoleExistsAsync(UserRoles.Doctor))
            await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Doctor));

        return services;
    }
}