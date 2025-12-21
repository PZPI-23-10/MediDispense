using Application.DTOs.Patient;

namespace Application.Interfaces.Services;

public interface IPatientsService
{
    Task<int> Create(CreatePatientDto dto);
    Task Update(int id, CreatePatientDto dto);
    Task<PatientDto> GetById(int id);
    Task<IEnumerable<PatientDto>> GetAll();
    Task Delete(int id);
}