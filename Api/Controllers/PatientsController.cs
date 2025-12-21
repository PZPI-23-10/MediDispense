using Application.DTOs.Patient;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PatientsController(IDataContext dataContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        return await dataContext.Patients
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth
            })
            .ToListAsync();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePatientDto dto)
    {
        var patient = await dataContext.Patients.FindAsync(id);

        if (patient == null)
            return NotFound();

        patient.FullName = dto.FullName;
        patient.DateOfBirth = dto.DateOfBirth;

        await dataContext.SaveChangesAsync(CancellationToken.None);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await dataContext.Patients
            .Include(p => p.Prescriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
            return NotFound();

        dataContext.Patients.Remove(patient);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePatientDto dto)
    {
        var patient = new Patient
        {
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
        };

        dataContext.Patients.Add(patient);
        await dataContext.SaveChangesAsync();

        return Ok(patient.Id);
    }
}