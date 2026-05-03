using Application.DTOs.Prescription;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IPrescriptionService
{
    Task<CreatePrescriptionResponse> Create(int userId, bool isAdmin, CreatePrescriptionRequest request);
    Task<PrescriptionResponseDto> Get(int id, int currentUserId, bool isAdmin);

    Task<IEnumerable<PrescriptionResponseDto>> GetAll(int? patientId = null,
        int? doctorId = null,
        int? status = null,
        int? currentUserId = null,
        bool isAdmin = false);

    Task<PrescriptionResponseDto> Update(int id, int currentUserId, bool isAdmin, UpdatePrescriptionRequest request);
    Task Cancel(int id, int currentUserId, bool isAdmin);
    Task<byte[]> GetQrCode(int id);
}
