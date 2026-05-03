using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class DataContext(DbContextOptions options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options), IDataContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Cell> Cells { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
    public DbSet<DispenseLog> DispenseLogs { get; set; }

    public override int SaveChanges()
    {
        UpdateDateTimeProperties();
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateDateTimeProperties();
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var e in entries)
        {
            var now = DateTimeOffset.UtcNow;

            if (e.State == EntityState.Added)
            {
                e.Property(x => x.Created).CurrentValue = now;
                e.Property(x => x.LastModified).CurrentValue = now;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Property(x => x.LastModified).CurrentValue = now;
            }
        }
    }

    private void UpdateDateTimeProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dateTime)
                {
                    property.CurrentValue = ToUtc(dateTime);
                }
                else if (property.CurrentValue is DateTimeOffset dateTimeOffset)
                {
                    property.CurrentValue = dateTimeOffset.ToUniversalTime();
                }
            }
        }
    }

    private static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>()
            .Property(device => device.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Prescription>()
            .Property(prescription => prescription.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DispenseLog>()
            .Property(log => log.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Device>()
            .HasMany(device => device.Cells)
            .WithOne(x => x.Device)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
