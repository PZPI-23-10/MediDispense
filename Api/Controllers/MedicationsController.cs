using Application.DTOs.Medication;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicationsController(IMedicationsService medicationsService) : ControllerBase
{
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin},{UserRoles.Doctor}")]
    public async Task<IEnumerable<MedicationDto>> GetAll()
    {
        return await medicationsService.GetAll();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<int>> Create(CreateMedicationDto dto)
    {
        var medicationId = await medicationsService.Create(dto);

        return Ok(new { medication = medicationId });
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMedicationDto dto)
    {
        await medicationsService.Update(id, dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await medicationsService.Delete(id);
        return NoContent();
    }
}
