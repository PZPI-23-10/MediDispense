using Application.DTOs.Dispense;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class ReportsController(IDataContext dataContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogDto>>> GetLogs([FromQuery] int? prescriptionId,
        [FromQuery] int? patientId, [FromQuery] int? deviceId)
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

        return Ok(logs);
    }
}