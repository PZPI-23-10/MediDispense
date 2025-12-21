using Application.DTOs.Dispense;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DispenseService(IDataContext dataContext) : IDispenseService
{
    public async Task<DispenseInstructionDto> Dispense(VerifyPrescriptionRequest request)
    {
        Device? device = await dataContext.Devices.FindAsync(request.DeviceId);

        if (device == null)
            throw new NotFoundException($"Device with id ({request.DeviceId}) not found");
        
        if (device.Status != DeviceStatus.Online)
            throw new InvalidOperationException($"Device with id ({request.DeviceId}) is offline");

        var prescription = await dataContext.Prescriptions
            .FirstOrDefaultAsync(p => p.PrescriptionGuid == request.PrescriptionGuid);

        if (prescription == null)
            throw new NotFoundException($"Prescription with guid ({request.PrescriptionGuid}) not found");

        if (prescription.Status != PrescriptionStatus.Active)
        {
            await CreateLogAsync(request.DeviceId, prescription.Id, DispenseStatus.InvalidPrescriptionState);

            throw new InvalidOperationException($"Prescription is {prescription.Status.ToString()}");
        }

        var instructions = new DispenseInstructionDto
        {
            PrescriptionId = prescription.Id,
        };

        foreach (var item in prescription.Medications)
        {
            var cell = device.Cells.FirstOrDefault(c =>
                c.MedicationId == item.MedicationId &&
                c.CurrentQuantity >= item.Quantity);

            if (cell == null)
            {
                await CreateLogAsync(request.DeviceId, prescription.Id, DispenseStatus.OutOfStock);

                throw new NotFoundException(
                    $"Medication '{item.Medication.Name}' is out of stock or not loaded in this device.");
            }

            instructions.ItemsToDispense.Add(new MedicationDispenseItem
            {
                CellId = cell.Id,
                CellLabel = cell.CellLabel,
                MedicationName = item.Medication.Name,
                Quantity = item.Quantity
            });
        }

        prescription.Status = PrescriptionStatus.Dispensing;

        dataContext.Prescriptions.Update(prescription);
        await dataContext.SaveChangesAsync();

        return instructions;
    }

    public async Task ConfirmDispense(ConfirmDispenseRequest request)
    {
        var prescription = await dataContext.Prescriptions.FindAsync(request.PrescriptionId);

        if (prescription == null)
            throw new NotFoundException($"Prescription with id ({request.PrescriptionId}) not found");

        if (prescription.Status != PrescriptionStatus.Dispensing)
            throw new InvalidOperationException("Prescription is not dispensing");

        DispenseStatus status = request.IsSuccess ? DispenseStatus.Success : DispenseStatus.SystemError;

        await CreateLogAsync(request.DeviceId, request.PrescriptionId, status);

        if (request.IsSuccess)
        {
            prescription.Status = PrescriptionStatus.Completed;

            var device = await dataContext.Devices.FindAsync(request.DeviceId);

            if (device == null)
                throw new NotFoundException($"Device with id ({request.DeviceId}) not found");

            foreach (PrescriptionMedication item in prescription.Medications)
            {
                var cell = device.Cells.FirstOrDefault(c => c.MedicationId == item.MedicationId);

                if (cell != null)
                    cell.CurrentQuantity = Math.Max(0, cell.CurrentQuantity - item.Quantity);
            }
        }
        else
        {
            prescription.Status = PrescriptionStatus.Active;
        }

        await dataContext.SaveChangesAsync();
    }

    private async Task CreateLogAsync(int deviceId, int prescriptionId, DispenseStatus status)
    {
        var log = new DispenseLog
        {
            DeviceId = deviceId,
            PrescriptionId = prescriptionId,
            Status = status,
        };

        dataContext.DispenseLogs.Add(log);
        await dataContext.SaveChangesAsync(CancellationToken.None);
    }
}