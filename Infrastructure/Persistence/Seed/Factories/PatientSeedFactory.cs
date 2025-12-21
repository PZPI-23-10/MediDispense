using Domain.Entities;

namespace Infrastructure.Persistence.Seed.Factories;

public static class PatientSeedFactory
{
    public static IEnumerable<Patient> CreateSeedData()
    {
        return new List<Patient>
        {
            new() { FullName = "John Doe", DateOfBirth = new DateTime(1980, 5, 15).ToUniversalTime() },
            new() { FullName = "Jane Smith", DateOfBirth = new DateTime(1992, 11, 20).ToUniversalTime() },
            new() { FullName = "Oleksandr Petrenko", DateOfBirth = new DateTime(1975, 3, 10).ToUniversalTime() },
            new() { FullName = "Maria Kovenko", DateOfBirth = new DateTime(2001, 7, 25).ToUniversalTime() }
        };
    }
}