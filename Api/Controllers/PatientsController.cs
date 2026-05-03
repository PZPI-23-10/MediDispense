using Api.Extensions;
using Application.DTOs.Patient;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin},{UserRoles.Doctor}")]
public class PatientsController(IPatientsService patientsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        IEnumerable<PatientDto> patients =
            await patientsService.GetAll(User.GetUserId(), User.IsInRole(UserRoles.Admin));

        return Ok(patients);
    }

    [HttpGet("my")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Doctor)]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetMy()
    {
        IEnumerable<PatientDto> patients = await patientsService.GetMy(User.GetUserId());

        return Ok(patients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> Get(int id)
    {
        PatientDto patient = await patientsService.GetById(id, User.GetUserId(), User.IsInRole(UserRoles.Admin));
        return Ok(patient);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePatientDto dto)
    {
        await patientsService.Update(id, User.GetUserId(), User.IsInRole(UserRoles.Admin), dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await patientsService.Delete(id);

        return NoContent();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<int>> Create(CreatePatientDto dto)
    {
        int patientId = await patientsService.Create(dto);
        return Ok(patientId);
    }
}
