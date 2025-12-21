using Application.DTOs.Prescription;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IPrescriptionService
{
    Task<CreatePrescriptionResponse> Create(int userId, CreatePrescriptionRequest request);
    Task<PrescriptionResponseDto> Get(int id);

    Task<IEnumerable<PrescriptionResponseDto>> GetAll(int? patientId = null,
        int? doctorId = null,
        int? status = null,
        int? currentUserId = null,
        string? currentUserRole = UserRoles.Doctor);

    Task Cancel(int id);
    Task<byte[]> GetQrCode(int id);
}