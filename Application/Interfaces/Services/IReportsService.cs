using Application.DTOs.Dispense;

namespace Application.Interfaces.Services;

public interface IReportsService
{
    Task<IEnumerable<LogDto>> GetLogs(int? prescriptionId = null, int? patientId = null, int? deviceId = null);
}