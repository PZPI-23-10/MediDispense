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
    public async Task<CreatePrescriptionResponse> Create(int userId, bool isAdmin, CreatePrescriptionRequest request)
    {
        if (request == null)
            throw new ValidationException("Request body is required");

        await ValidatePrescriptionPayload(request.PatientId, request.Medications);

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

    public async Task<PrescriptionResponseDto> Get(int id, int currentUserId, bool isAdmin)
    {
        var prescription = await dataContext.Prescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prescription == null)
            throw new NotFoundException("Prescription not found");

        if (!isAdmin && prescription.DoctorId != currentUserId)
            throw new ForbiddenAccessException("Prescription does not belong to current doctor");

        return await ProjectPrescription(dataContext.Prescriptions.Where(p => p.Id == id)).SingleAsync();
    }

    public async Task<IEnumerable<PrescriptionResponseDto>> GetAll(
        int? patientId = null,
        int? doctorId = null,
        int? status = null,
        int? currentUserId = null,
        bool isAdmin = false)
    {
        var query = dataContext.Prescriptions.AsQueryable();

        if (!isAdmin)
        {
            if (currentUserId == null)
                throw new UnauthorizedException("User ID is required");

            query = query.Where(p => p.DoctorId == currentUserId);
        }

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

        return await ProjectPrescription(query)
            .OrderByDescending(p => p.Created)
            .ToListAsync();
    }

    public async Task<PrescriptionResponseDto> Update(
        int id,
        int currentUserId,
        bool isAdmin,
        UpdatePrescriptionRequest request)
    {
        if (request == null)
            throw new ValidationException("Request body is required");

        await ValidatePrescriptionPayload(request.PatientId, request.Medications);

        var prescription = await dataContext.Prescriptions
            .Include(p => p.Medications)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prescription == null)
            throw new NotFoundException("Prescription not found");

        if (!isAdmin && prescription.DoctorId != currentUserId)
            throw new ForbiddenAccessException("Prescription does not belong to current doctor");

        if (prescription.Status != PrescriptionStatus.Active)
            throw new ConflictException("Only active prescriptions can be updated");

        dataContext.PrescriptionMedications.RemoveRange(prescription.Medications);

        prescription.PatientId = request.PatientId;
        prescription.Medications = request.Medications.Select(m => new PrescriptionMedication
        {
            MedicationId = m.MedicationId,
            Quantity = m.Quantity
        }).ToList();

        await dataContext.SaveChangesAsync(CancellationToken.None);

        return await ProjectPrescription(dataContext.Prescriptions.Where(p => p.Id == id)).SingleAsync();
    }

    public async Task Cancel(int id, int currentUserId, bool isAdmin)
    {
        var prescription = await dataContext.Prescriptions.FindAsync(id);

        if (prescription == null)
            throw new NotFoundException("Prescription not found");

        if (!isAdmin && prescription.DoctorId != currentUserId)
            throw new ForbiddenAccessException("Prescription does not belong to current doctor");

        if (prescription.Status != PrescriptionStatus.Active)
            throw new ConflictException("Only active prescriptions can be canceled");

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

    private static IQueryable<PrescriptionResponseDto> ProjectPrescription(IQueryable<Prescription> query)
    {
        return query.Select(p => new PrescriptionResponseDto
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
        });
    }

    private async Task ValidatePrescriptionPayload(int patientId, List<PrescriptionItemDto>? medications)
    {
        if (!await dataContext.Patients.AnyAsync(p => p.Id == patientId))
            throw new ValidationException("Patient does not exist");

        if (medications == null || medications.Count == 0)
            throw new ValidationException("Medications list cannot be empty");

        if (medications.Any(m => m.Quantity <= 0))
            throw new ValidationException("Medication quantity must be greater than zero");

        if (medications.Select(m => m.MedicationId).Distinct().Count() != medications.Count)
            throw new ValidationException("Medication list contains duplicates");

        var medicationIds = medications.Select(m => m.MedicationId).Distinct().ToList();
        var existingMedicationIds = await dataContext.Medications
            .Where(m => medicationIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();

        if (existingMedicationIds.Count != medicationIds.Count)
            throw new ValidationException("One or more medications do not exist");
    }

}
