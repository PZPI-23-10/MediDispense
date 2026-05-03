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
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Admin));

        if (!await roleManager.RoleExistsAsync(UserRoles.Doctor))
            await roleManager.CreateAsync(new IdentityRole<int>(UserRoles.Doctor));

        var admin = await userManager.FindByNameAsync("admin");

        if (admin == null)
        {
            admin = new User
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                FullName = "Admin",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
        }

        admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, "admin");

        if (admin.Id == 0)
        {
            IdentityResult createResult = await userManager.CreateAsync(admin);

            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    $"Could not seed admin user: {string.Join(',', createResult.Errors.Select(e => e.Code))}");
        }
        else
        {
            IdentityResult updateResult = await userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
                throw new InvalidOperationException(
                    $"Could not update admin user: {string.Join(',', updateResult.Errors.Select(e => e.Code))}");
        }

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
        {
            IdentityResult roleResult = await userManager.AddToRoleAsync(admin, UserRoles.Admin);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Could not add admin role: {string.Join(',', roleResult.Errors.Select(e => e.Code))}");
        }

        return services;
    }
}
