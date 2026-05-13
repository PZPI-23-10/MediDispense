using Api.Extensions;
using Application.Extensions;
using Application.Validation;
using FluentValidation;
using Infrastructure.Extensions;
using Infrastructure.Persistence;

namespace Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddAuthorization();

        builder.Services.ConfigureIdentity();

        builder.Services.AddApiControllers();

        builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserRequestValidator).Assembly);

        builder.Services.AddHostedService<DeviceStatusMonitor>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerDocumentation();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        builder.Services.AddPersistence(builder.Configuration);

        builder.Services.ConfigureCorsPolicy();
        builder.Services.AddMemoryCache();

        WebApplication app = builder.Build();

        await using AsyncServiceScope serviceScope = app.Services.CreateAsyncScope();
        await using DataContext dataContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
        await dataContext.Database.EnsureCreatedAsync();

        await app.Services.SeedIdentity();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseErrorHandler();
        MapDiagnosticsEndpoints(app);
        app.MapControllers();

        await app.RunAsync();
    }

    private static void MapDiagnosticsEndpoints(WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Instance = GetInstanceName(),
            TimestampUtc = DateTimeOffset.UtcNow
        }));

        app.MapGet("/api/diagnostics/instance", () => Results.Ok(new
        {
            Instance = GetInstanceName(),
            MachineName = Environment.MachineName,
            TimestampUtc = DateTimeOffset.UtcNow
        }));

        app.MapGet("/api/diagnostics/work", (int? iterations) =>
        {
            int workIterations = Math.Clamp(iterations ?? 750_000, 10_000, 5_000_000);
            ulong checksum = 1469598103934665603;

            unchecked
            {
                for (int i = 0; i < workIterations; i++)
                {
                    checksum ^= (uint)i;
                    checksum *= 1099511628211;
                    checksum ^= checksum >> 32;
                }
            }

            return Results.Ok(new
            {
                Instance = GetInstanceName(),
                Iterations = workIterations,
                Checksum = checksum.ToString("x"),
                TimestampUtc = DateTimeOffset.UtcNow
            });
        });
    }

    private static string GetInstanceName()
    {
        return Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName;
    }
}
