using Application.DTOs.Medication;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicationsController(IDataContext dataContext) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<MedicationDto>> GetAll()
    {
        return await dataContext.Medications
            .Select(m => new MedicationDto { Id = m.Id, Name = m.Name, Description = m.Description })
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<int>> Create(CreateMedicationDto dto)
    {
        var medication = new Medication { Name = dto.Name, Description = dto.Description };
        dataContext.Medications.Add(medication);

        await dataContext.SaveChangesAsync();
        return Ok(new { medication.Id });
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMedicationDto dto)
    {
        var med = await dataContext.Medications.FindAsync(id);
        if (med == null) return NotFound();

        med.Name = dto.Name;
        med.Description = dto.Description;

        await dataContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var isUsed = await dataContext.PrescriptionMedications.AnyAsync(pm => pm.MedicationId == id) ||
                     await dataContext.Cells.AnyAsync(c => c.MedicationId == id);

        if (isUsed)
            return BadRequest("Cannot delete medication that is currently in use (Prescriptions or Cells).");

        var medication = await dataContext.Medications.FindAsync(id);
        if (medication == null)
            return NotFound();

        dataContext.Medications.Remove(medication);
        await dataContext.SaveChangesAsync();
        return NoContent();
    }
}