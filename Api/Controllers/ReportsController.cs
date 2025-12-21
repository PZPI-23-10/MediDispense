using Application.DTOs.Dispense;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class ReportsController(IReportsService reportsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogDto>>> GetLogs([FromQuery] int? prescriptionId,
        [FromQuery] int? patientId, [FromQuery] int? deviceId)
    {
        IEnumerable<LogDto> logs = await reportsService.GetLogs(prescriptionId, patientId, deviceId);

        return Ok(logs);
    }
}