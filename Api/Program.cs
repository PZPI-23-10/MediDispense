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
        app.MapControllers();

        await app.RunAsync();
    }
}