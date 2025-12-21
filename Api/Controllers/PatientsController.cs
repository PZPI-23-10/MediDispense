using Application.DTOs.Patient;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PatientsController(IPatientsService patientsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        IEnumerable<PatientDto> patients = await patientsService.GetAll();
        return Ok(patients);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePatientDto dto)
    {
        await patientsService.Update(id, dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await patientsService.Delete(id);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePatientDto dto)
    {
        int patientId = await patientsService.Create(dto);
        return Ok(patientId);
    }
}