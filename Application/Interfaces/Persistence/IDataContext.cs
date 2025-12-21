using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces.Persistence;

public interface IDataContext
{
    DbSet<Patient> Patients { get; set; }
    DbSet<Medication> Medications { get; set; }
    DbSet<Device> Devices { get; set; }
    DbSet<Cell> Cells { get; set; }
    DbSet<Prescription> Prescriptions { get; set; }
    DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
    DbSet<DispenseLog> DispenseLogs { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<IdentityUserClaim<int>> UserClaims { get; set; }
    DbSet<IdentityUserLogin<int>> UserLogins { get; set; }
    DbSet<IdentityUserToken<int>> UserTokens { get; set; }
    DbSet<IdentityUserRole<int>> UserRoles { get; set; }
    DbSet<IdentityRole<int>> Roles { get; set; }
    DbSet<IdentityRoleClaim<int>> RoleClaims { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}