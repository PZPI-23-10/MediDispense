using Application.DTOs.Patient;

namespace Application.Interfaces.Services;

public interface IPatientsService
{
    Task<int> Create(CreatePatientDto dto);
    Task Update(int id, int currentUserId, bool isAdmin, CreatePatientDto dto);
    Task<PatientDto> GetById(int id, int currentUserId, bool isAdmin);
    Task<IEnumerable<PatientDto>> GetAll(int currentUserId, bool isAdmin);
    Task<IEnumerable<PatientDto>> GetMy(int doctorId);
    Task Delete(int id);
}
