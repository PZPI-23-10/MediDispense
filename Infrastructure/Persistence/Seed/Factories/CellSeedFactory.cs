using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed.Factories;

public static class CellSeedFactory
{
    public static async Task<IEnumerable<Cell>> CreateSeedData(DbContext context, CancellationToken ct)
    {
        var devices = await context.Set<Device>().ToListAsync(ct);
        var meds = await context.Set<Medication>().ToListAsync(ct);

        if (!devices.Any() || !meds.Any()) return [];

        var cells = new List<Cell>();

        var mainDevice = devices.First(d => d.Title == "Main Hall Dispenser");

        cells.Add(new Cell
        {
            DeviceId = mainDevice.Id,
            CellLabel = "A1",
            MedicationId = meds.First(m => m.Name == "Aspirin").Id,
            CurrentQuantity = 50
        });

        cells.Add(new Cell
        {
            DeviceId = mainDevice.Id,
            CellLabel = "A2",
            MedicationId = meds.First(m => m.Name == "Ibuprofen").Id,
            CurrentQuantity = 30
        });

        cells.Add(new Cell
        {
            DeviceId = mainDevice.Id,
            CellLabel = "B1",
            MedicationId = meds.First(m => m.Name == "Paracetamol").Id,
            CurrentQuantity = 100
        });

        return cells;
    }
}