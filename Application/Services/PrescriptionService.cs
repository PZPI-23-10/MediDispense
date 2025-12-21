using System.ComponentModel.DataAnnotations;
using Application.DTOs.Prescription;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class PrescriptionService(IDataContext dataContext, IQrCodeGenerator qrCodeGenerator) : IPrescriptionService
{
    public async Task<CreatePrescriptionResponse> Create(int userId, CreatePrescriptionRequest request)
    {
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

        return new CreatePrescriptionResponse
        {
            Id = prescription.Id, PrescriptionGuid = prescription.PrescriptionGuid.ToString()
        };
    }

    public async Task<PrescriptionResponseDto> Get(int id)
    {
        var prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            throw new NotFoundException("Prescription not found");

        return new PrescriptionResponseDto
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
        };
    }

    public async Task<IEnumerable<PrescriptionResponseDto>> GetAll(
        int? patientId = null,
        int? doctorId = null,
        int? status = null,
        int? currentUserId = null,
        string? currentUserRole = UserRoles.Doctor)
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

        return result;
    }

    public async Task Cancel(int id)
    {
        var prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            throw new NotFoundException("Prescription not found");

        if (prescription.Status != PrescriptionStatus.Active)
            throw new ValidationException("Cannot cancel prescription");

        prescription.Status = PrescriptionStatus.Canceled;

        dataContext.Prescriptions.Update(prescription);
        await dataContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task<byte[]> GetQrCode(int id)
    {
        Prescription? prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            throw new NotFoundException("Prescription not found");

        var payload = prescription.PrescriptionGuid.ToString();
        byte[] qrCode = qrCodeGenerator.GenerateQrCode(payload);

        return qrCode;
    }
}