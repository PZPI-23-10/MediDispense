using Application.DTOs.Dispense;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ReportsService(IDataContext dataContext) : IReportsService
{
    public async Task<IEnumerable<LogDto>> GetLogs(int? prescriptionId = null, int? patientId = null,
        int? deviceId = null)
    {
        var query = dataContext.DispenseLogs.AsQueryable();

        if (prescriptionId.HasValue)
            query = query.Where(x => x.PrescriptionId == prescriptionId);

        if (deviceId.HasValue)
            query = query.Where(l => l.DeviceId == deviceId);

        if (patientId.HasValue)
            query = query.Where(l => l.Prescription.PatientId == patientId);

        List<LogDto> logs = await query
            .OrderByDescending(l => l.Created)
            .Select(l => new LogDto
            {
                Id = l.Id,
                Device = l.Device.Title,
                Patient = l.Prescription.Patient.FullName,
                MedicationCount = l.Prescription.Medications.Count,
                Status = l.Status.ToString(),
                Timestamp = l.Created
            })
            .ToListAsync();

        return logs;
    }
}