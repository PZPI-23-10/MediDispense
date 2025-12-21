using Domain.Entities;
using Infrastructure.Persistence.Seed.Factories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task Seed(DbContext context, bool flag, CancellationToken cancellationToken = default)
    {
        if (!await context.Set<Medication>().AnyAsync(cancellationToken))
        {
            await context.Set<Medication>().AddRangeAsync(MedicationSeedFactory.CreateSeedData(), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<Patient>().AnyAsync(cancellationToken))
        {
            await context.Set<Patient>().AddRangeAsync(PatientSeedFactory.CreateSeedData(), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<Device>().AnyAsync(cancellationToken))
        {
            await context.Set<Device>().AddRangeAsync(DeviceSeedFactory.CreateSeedData(), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<Cell>().AnyAsync(cancellationToken))
        {
            var cellData = await CellSeedFactory.CreateSeedData(context, cancellationToken);

            await context.Set<Cell>().AddRangeAsync(cellData, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}