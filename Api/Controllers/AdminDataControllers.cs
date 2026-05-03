using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.DTOs.AdminData;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
public class BackupController(IDataContext dataContext, UserManager<User> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BackupResponseDto>> CreateBackup()
    {
        return Ok(new BackupResponseDto
        {
            CreatedUtc = DateTimeOffset.UtcNow,
            Data = await AdminDataSnapshot.Build(dataContext, userManager)
        });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
public class ExportController(IDataContext dataContext, UserManager<User> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Export([FromQuery] string format = "json")
    {
        AdminDataDto snapshot = await AdminDataSnapshot.Build(dataContext, userManager);

        return format.ToLowerInvariant() switch
        {
            "json" => Ok(snapshot),
            "csv" => Content(AdminDataSnapshot.ToCsv(snapshot), "text/csv", Encoding.UTF8),
            _ => BadRequest(new { message = "Unsupported export format. Use json or csv." })
        };
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
public class ImportController(IDataContext dataContext, UserManager<User> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ImportResultDto>> Import([FromBody] AdminDataDto data)
    {
        if (data == null)
            throw new ValidationException("Import payload is required");

        var result = new ImportResultDto();

        result.UsersProcessed = await ImportUsers(data.Users);
        result.PatientsProcessed = await ImportPatients(data.Patients);
        result.MedicationsProcessed = await ImportMedications(data.Medications);
        (result.DevicesProcessed, result.CellsProcessed) = await ImportDevices(data.Devices);
        result.PrescriptionsProcessed = await ImportPrescriptions(data.Prescriptions);

        return Ok(result);
    }

    private async Task<int> ImportUsers(IEnumerable<AdminUserDto> users)
    {
        var processed = 0;

        foreach (AdminUserDto dto in users)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ValidationException("User username is required");

            var user = dto.Id > 0
                ? await userManager.Users.FirstOrDefaultAsync(u => u.Id == dto.Id)
                : null;

            user ??= await userManager.FindByNameAsync(dto.Username);

            if (user == null)
            {
                user = new User
                {
                    Id = dto.Id,
                    UserName = dto.Username,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    EmailConfirmed = true
                };

                IdentityResult createResult = await userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                    throw new ValidationException(
                        $"Could not import user '{dto.Username}': {string.Join(',', createResult.Errors.Select(e => e.Code))}");
            }
            else
            {
                user.UserName = dto.Username;
                user.Email = dto.Email;
                user.FullName = dto.FullName;

                IdentityResult updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                    throw new ValidationException(
                        $"Could not update user '{dto.Username}': {string.Join(',', updateResult.Errors.Select(e => e.Code))}");
            }

            foreach (string role in dto.Roles.Distinct())
            {
                if (role != UserRoles.Admin && role != UserRoles.Doctor)
                    throw new ValidationException($"Role '{role}' is not supported");

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    IdentityResult roleResult = await userManager.AddToRoleAsync(user, role);

                    if (!roleResult.Succeeded)
                        throw new ValidationException(
                            $"Could not add role '{role}' to '{dto.Username}': {string.Join(',', roleResult.Errors.Select(e => e.Code))}");
                }
            }

            processed++;
        }

        return processed;
    }

    private async Task<int> ImportPatients(IEnumerable<AdminPatientDto> patients)
    {
        var processed = 0;

        foreach (AdminPatientDto dto in patients)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ValidationException("Patient fullName is required");

            var patient = dto.Id > 0
                ? await dataContext.Patients.FindAsync(dto.Id)
                : null;

            if (patient == null)
            {
                patient = new Patient { Id = dto.Id };
                dataContext.Patients.Add(patient);
            }

            patient.FullName = dto.FullName;
            patient.DateOfBirth = dto.DateOfBirth;
            processed++;
        }

        await dataContext.SaveChangesAsync(CancellationToken.None);

        return processed;
    }

    private async Task<int> ImportMedications(IEnumerable<AdminMedicationDto> medications)
    {
        var processed = 0;

        foreach (AdminMedicationDto dto in medications)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("Medication name is required");

            var medication = dto.Id > 0
                ? await dataContext.Medications.FindAsync(dto.Id)
                : null;

            if (medication == null)
            {
                medication = new Medication { Id = dto.Id };
                dataContext.Medications.Add(medication);
            }

            medication.Name = dto.Name;
            medication.Description = dto.Description;
            processed++;
        }

        await dataContext.SaveChangesAsync(CancellationToken.None);

        return processed;
    }

    private async Task<(int devices, int cells)> ImportDevices(IEnumerable<AdminDeviceDto> devices)
    {
        var processedDevices = 0;
        var processedCells = 0;

        foreach (AdminDeviceDto dto in devices)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ValidationException("Device title is required");

            if (!Enum.TryParse(dto.Status, true, out DeviceStatus status))
                throw new ValidationException($"Device status '{dto.Status}' is invalid");

            var device = dto.Id > 0
                ? await dataContext.Devices.FindAsync(dto.Id)
                : null;

            if (device == null)
            {
                device = new Device { Id = dto.Id };
                dataContext.Devices.Add(device);
            }

            device.Title = dto.Title;
            device.Status = status;
            device.LastActive = dto.LastActive;
            processedDevices++;

            var labels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AdminCellDto cellDto in dto.Cells)
            {
                if (string.IsNullOrWhiteSpace(cellDto.Label))
                    throw new ValidationException("Cell label is required");

                if (!labels.Add(cellDto.Label))
                    throw new ValidationException($"Device {dto.Id} contains duplicate cell label '{cellDto.Label}'");

                if (cellDto.Quantity < 0)
                    throw new ValidationException("Cell quantity cannot be negative");

                if (cellDto.MedicationId.HasValue &&
                    !await dataContext.Medications.AnyAsync(m => m.Id == cellDto.MedicationId.Value))
                    throw new ValidationException($"Medication {cellDto.MedicationId.Value} does not exist");

                var cell = cellDto.Id > 0
                    ? await dataContext.Cells.FindAsync(cellDto.Id)
                    : null;

                if (cell == null)
                {
                    cell = new Cell { Id = cellDto.Id };
                    dataContext.Cells.Add(cell);
                }

                cell.DeviceId = dto.Id;
                cell.CellLabel = cellDto.Label;
                cell.MedicationId = cellDto.MedicationId;
                cell.CurrentQuantity = cellDto.Quantity;
                processedCells++;
            }
        }

        await dataContext.SaveChangesAsync(CancellationToken.None);

        return (processedDevices, processedCells);
    }

    private async Task<int> ImportPrescriptions(IEnumerable<AdminPrescriptionDto> prescriptions)
    {
        var processed = 0;

        foreach (AdminPrescriptionDto dto in prescriptions)
        {
            if (!await dataContext.Patients.AnyAsync(p => p.Id == dto.PatientId))
                throw new ValidationException($"Patient {dto.PatientId} does not exist");

            if (!await dataContext.Users.AnyAsync(u => u.Id == dto.DoctorId))
                throw new ValidationException($"Doctor {dto.DoctorId} does not exist");

            if (!Enum.TryParse(dto.Status, true, out PrescriptionStatus status))
                throw new ValidationException($"Prescription status '{dto.Status}' is invalid");

            var prescription = dto.Id > 0
                ? await dataContext.Prescriptions
                    .Include(p => p.Medications)
                    .FirstOrDefaultAsync(p => p.Id == dto.Id)
                : null;

            if (prescription == null)
            {
                prescription = new Prescription { Id = dto.Id };
                dataContext.Prescriptions.Add(prescription);
            }
            else
            {
                dataContext.PrescriptionMedications.RemoveRange(prescription.Medications);
            }

            prescription.PatientId = dto.PatientId;
            prescription.DoctorId = dto.DoctorId;
            prescription.PrescriptionGuid = dto.PrescriptionGuid == Guid.Empty
                ? Guid.NewGuid()
                : dto.PrescriptionGuid;
            prescription.Status = status;

            foreach (AdminPrescriptionMedicationDto item in dto.Medications)
            {
                if (item.Quantity <= 0)
                    throw new ValidationException("Prescription medication quantity must be greater than zero");

                if (!await dataContext.Medications.AnyAsync(m => m.Id == item.MedicationId))
                    throw new ValidationException($"Medication {item.MedicationId} does not exist");

                prescription.Medications.Add(new PrescriptionMedication
                {
                    MedicationId = item.MedicationId,
                    Quantity = item.Quantity
                });
            }

            processed++;
        }

        await dataContext.SaveChangesAsync(CancellationToken.None);

        return processed;
    }
}

internal static class AdminDataSnapshot
{
    public static async Task<AdminDataDto> Build(IDataContext dataContext, UserManager<User> userManager)
    {
        var users = new List<AdminUserDto>();

        foreach (User user in await userManager.Users.AsNoTracking().ToListAsync())
        {
            users.Add(new AdminUserDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = (await userManager.GetRolesAsync(user)).ToList()
            });
        }

        return new AdminDataDto
        {
            Users = users,
            Patients = await dataContext.Patients
                .AsNoTracking()
                .Select(p => new AdminPatientDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth
                })
                .ToListAsync(),
            Medications = await dataContext.Medications
                .AsNoTracking()
                .Select(m => new AdminMedicationDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description
                })
                .ToListAsync(),
            Devices = await dataContext.Devices
                .AsNoTracking()
                .Select(d => new AdminDeviceDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Status = d.Status.ToString(),
                    LastActive = d.LastActive,
                    Cells = d.Cells.Select(c => new AdminCellDto
                    {
                        Id = c.Id,
                        DeviceId = c.DeviceId,
                        Label = c.CellLabel,
                        MedicationId = c.MedicationId,
                        Quantity = c.CurrentQuantity
                    }).ToList()
                })
                .ToListAsync(),
            Prescriptions = await dataContext.Prescriptions
                .AsNoTracking()
                .Select(p => new AdminPrescriptionDto
                {
                    Id = p.Id,
                    PrescriptionGuid = p.PrescriptionGuid,
                    PatientId = p.PatientId,
                    DoctorId = p.DoctorId,
                    Status = p.Status.ToString(),
                    Created = p.Created,
                    Medications = p.Medications.Select(pm => new AdminPrescriptionMedicationDto
                    {
                        MedicationId = pm.MedicationId,
                        Quantity = pm.Quantity
                    }).ToList()
                })
                .ToListAsync()
        };
    }

    public static string ToCsv(AdminDataDto data)
    {
        var csv = new StringBuilder();
        csv.AppendLine("entity,id,name,details");

        foreach (AdminUserDto user in data.Users)
            AppendCsvLine(csv, "user", user.Id, user.Username, string.Join('|', user.Roles));

        foreach (AdminPatientDto patient in data.Patients)
            AppendCsvLine(csv, "patient", patient.Id, patient.FullName, patient.DateOfBirth.ToString("O"));

        foreach (AdminMedicationDto medication in data.Medications)
            AppendCsvLine(csv, "medication", medication.Id, medication.Name, medication.Description);

        foreach (AdminDeviceDto device in data.Devices)
        {
            AppendCsvLine(csv, "device", device.Id, device.Title, device.Status);

            foreach (AdminCellDto cell in device.Cells)
                AppendCsvLine(csv, "cell", cell.Id, cell.Label, $"device={cell.DeviceId};medication={cell.MedicationId};quantity={cell.Quantity}");
        }

        foreach (AdminPrescriptionDto prescription in data.Prescriptions)
            AppendCsvLine(csv, "prescription", prescription.Id, prescription.PrescriptionGuid.ToString(), prescription.Status);

        return csv.ToString();
    }

    private static void AppendCsvLine(StringBuilder csv, string entity, int id, string name, string details)
    {
        csv.Append(Escape(entity));
        csv.Append(',');
        csv.Append(id);
        csv.Append(',');
        csv.Append(Escape(name));
        csv.Append(',');
        csv.Append(Escape(details));
        csv.AppendLine();
    }

    private static string Escape(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
