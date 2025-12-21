using Application.Interfaces.Services;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDispenseService, DispenseService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IPatientsService, PatientsService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IMedicationsService, MedicationService>();

        return services;
    }
}