using Domain.Entities;

namespace Infrastructure.Persistence.Seed.Factories;

public static class MedicationSeedFactory
{
    public static IEnumerable<Medication> CreateSeedData()
    {
        return new List<Medication>
        {
            new() { Name = "Aspirin", Description = "Analgesic and antipyretic, 500mg" },
            new() { Name = "Ibuprofen", Description = "Anti-inflammatory, 200mg" },
            new() { Name = "Paracetamol", Description = "Pain reliever, 500mg" },
            new() { Name = "Amoxicillin", Description = "Antibiotic, 250mg" },
            new() { Name = "Metformin", Description = "Antidiabetic, 850mg" },
            new() { Name = "Omeprazole", Description = "Gastro-resistant, 20mg" }
        };
    }
}