using System.ComponentModel.DataAnnotations;
using Application.DTOs.Medication;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class MedicationService(IDataContext dataContext) : IMedicationsService
{
    public async Task<IEnumerable<MedicationDto>> GetAll()
    {
        return await dataContext.Medications
            .Select(m => new MedicationDto { Id = m.Id, Name = m.Name, Description = m.Description })
            .ToListAsync();
    }

    public async Task<int> Create(CreateMedicationDto dto)
    {
        var medication = new Medication { Name = dto.Name, Description = dto.Description };
        dataContext.Medications.Add(medication);

        await dataContext.SaveChangesAsync();

        return medication.Id;
    }

    public async Task Update(int id, CreateMedicationDto dto)
    {
        var medications = await dataContext.Medications.FindAsync(id);

        if (medications == null)
            throw new NotFoundException("Medication not found");

        medications.Name = dto.Name;
        medications.Description = dto.Description;

        await dataContext.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var isUsed = await dataContext.PrescriptionMedications.AnyAsync(pm => pm.MedicationId == id) ||
                     await dataContext.Cells.AnyAsync(c => c.MedicationId == id);

        if (isUsed)
        {
            throw new ValidationException(
                "Cannot delete medication that is currently in use (Prescriptions or Cells).");
        }

        var medication = await dataContext.Medications.FindAsync(id);
        if (medication == null)
            throw new NotFoundException("Medication not found");

        dataContext.Medications.Remove(medication);
        await dataContext.SaveChangesAsync();
    }
}