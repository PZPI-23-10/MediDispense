using Domain.Entities;

namespace Infrastructure.Persistence.Seed.Factories;

public static class DeviceSeedFactory
{
    public static IEnumerable<Device> CreateSeedData()
    {
        return new List<Device>
        {
            new() { Title = "Main Hall Dispenser", Status = DeviceStatus.Online },
            new() { Title = "ICU Unit 3", Status = DeviceStatus.Offline },
            new() { Title = "Emergency Room Post", Status = DeviceStatus.Online }
        };
    }
}