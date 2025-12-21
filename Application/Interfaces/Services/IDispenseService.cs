using Application.DTOs.Dispense;

namespace Application.Interfaces.Services;

public interface IDispenseService
{
    Task<DispenseInstructionDto> Dispense(VerifyPrescriptionRequest request);
    Task ConfirmDispense(ConfirmDispenseRequest request);
}