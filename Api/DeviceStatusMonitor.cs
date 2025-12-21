using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class DeviceStatusMonitor(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _timeoutThreshold = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDevicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeviceStatusMonitor: {ex.Message}");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckDevicesAsync(CancellationToken token)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDataContext>();

        var cutoffTime = DateTime.UtcNow.Subtract(_timeoutThreshold);

        await context.Devices
            .Where(d => d.Status == DeviceStatus.Online && d.LastActive < cutoffTime)
            .ExecuteUpdateAsync(
                s => s.SetProperty(d => d.Status, DeviceStatus.Offline),
                token);
    }
}