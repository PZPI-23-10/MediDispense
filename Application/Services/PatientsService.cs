using Application.DTOs.Patient;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class PatientsService(IDataContext dataContext) : IPatientsService
{
    public async Task<int> Create(CreatePatientDto dto)
    {
        var patient = new Patient
        {
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
        };

        dataContext.Patients.Add(patient);
        await dataContext.SaveChangesAsync();

        return patient.Id;
    }

    public async Task Update(int id, CreatePatientDto dto)
    {
        var patient = await dataContext.Patients.FindAsync(id);

        if (patient == null)
            throw new NotFoundException("Patient not found");

        patient.FullName = dto.FullName;
        patient.DateOfBirth = dto.DateOfBirth;

        await dataContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task<PatientDto> GetById(int id)
    {
        Patient? patient = await dataContext.Patients.FindAsync(id);

        if (patient == null)
            throw new NotFoundException("Patient not found");

        return new PatientDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
        };
    }

    public async Task<IEnumerable<PatientDto>> GetAll()
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

    public async Task Delete(int id)
    {
        var patient = await dataContext.Patients
            .Include(p => p.Prescriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
            throw new NotFoundException("Patient not found");

        dataContext.Patients.Remove(patient);
        await dataContext.SaveChangesAsync(CancellationToken.None);
    }
}