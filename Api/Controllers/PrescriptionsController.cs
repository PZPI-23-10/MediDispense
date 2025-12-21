using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Api.Extensions;
using Application.DTOs.Prescription;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PrescriptionsController(IDataContext dataContext, IQrCodeGenerator qrCodeGenerator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreatePrescriptionResponse>> Create(CreatePrescriptionRequest request)
    {
        var userId = User.GetUserId();

        var prescription = new Prescription
        {
            PatientId = request.PatientId,
            DoctorId = userId,
            PrescriptionGuid = Guid.NewGuid(),
            Status = PrescriptionStatus.Active,
            Medications = request.Medications.Select(m => new PrescriptionMedication
            {
                MedicationId = m.MedicationId,
                Quantity = m.Quantity
            }).ToList()
        };

        dataContext.Prescriptions.Add(prescription);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return Ok(new CreatePrescriptionResponse
            { Id = prescription.Id, PrescriptionGuid = prescription.PrescriptionGuid.ToString() });
    }

    [HttpGet]
    [Route("qr/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQrCode(int id)
    {
        Prescription? prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            return NotFound();

        var payload = prescription.PrescriptionGuid.ToString();
        byte[] qrCode = qrCodeGenerator.GenerateQrCode(payload);

        return File(qrCode, "image/jpeg");
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<PrescriptionResponseDto>> Get(int id)
    {
        var prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            return NotFound();

        return Ok(new PrescriptionResponseDto
        {
            Id = prescription.Id,
            PrescriptionGuid = prescription.PrescriptionGuid,
            PatientId = prescription.PatientId,
            PatientName = prescription.Patient.FullName,
            DoctorId = prescription.DoctorId,
            DoctorName = prescription.Doctor.FullName,
            Status = prescription.Status.ToString(),
            Created = prescription.Created,
            Medications = prescription.Medications.Select(pm => new PrescriptionItemDto
            {
                MedicationId = pm.MedicationId,
                Quantity = pm.Quantity
            }).ToList()
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<PrescriptionResponseDto>>> GetAll(
        [FromQuery] int? patientId,
        [FromQuery] int? doctorId,
        [FromQuery] int? status)
    {
        var query = dataContext.Prescriptions.AsQueryable();

        if (patientId.HasValue)
        {
            query = query.Where(p => p.PatientId == patientId.Value);
        }

        if (doctorId.HasValue)
        {
            query = query.Where(p => p.DoctorId == doctorId.Value);
        }

        if (status is not null)
        {
            query = query.Where(p => p.Status == (PrescriptionStatus)status.Value);
        }

        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var currentUserId = User.GetUserId();

        if (currentUserRole == UserRoles.Doctor && !patientId.HasValue && !doctorId.HasValue)
        {
            query = query.Where(p => p.DoctorId == currentUserId);
        }

        var result = await query
            .OrderByDescending(p => p.Created)
            .Select(p => new PrescriptionResponseDto
            {
                Id = p.Id,
                PrescriptionGuid = p.PrescriptionGuid,
                PatientId = p.PatientId,
                PatientName = p.Patient.FullName,
                DoctorId = p.DoctorId,
                DoctorName = p.Doctor.FullName,
                Status = p.Status.ToString(),
                Created = p.Created,
                Medications = p.Medications.Select(pm => new PrescriptionItemDto
                {
                    MedicationId = pm.MedicationId,
                    Quantity = pm.Quantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    [Route("cancel")]
    public async Task<IActionResult> Cancel([FromBody] int id)
    {
        var prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            return NotFound();

        if (prescription.Status != PrescriptionStatus.Active)
            throw new ValidationException("Cannot cancel prescription");

        prescription.Status = PrescriptionStatus.Canceled;

        dataContext.Prescriptions.Update(prescription);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return Ok();
    }
}