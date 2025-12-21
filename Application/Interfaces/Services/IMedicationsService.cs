using Application.DTOs.Medication;

namespace Application.Interfaces.Services;

public interface IMedicationsService
{
    Task<IEnumerable<MedicationDto>> GetAll();
    Task<int> Create(CreateMedicationDto dto);
    Task Update(int id, CreateMedicationDto dto);
    Task Delete(int id);
}