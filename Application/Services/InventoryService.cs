using System.ComponentModel.DataAnnotations;
using Application.DTOs.Device;
using Application.DTOs.Inventory;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class InventoryService(IDataContext dataContext) : IInventoryService
{
    public async Task<IEnumerable<AvailableMedicationDto>> GetAvailableMedications()
    {
        return await dataContext.Cells
            .Where(c => c.MedicationId != null && c.CurrentQuantity > 0)
            .Select(c => new AvailableMedicationDto
            {
                DeviceId = c.DeviceId,
                DeviceTitle = c.Device.Title,
                DeviceStatus = c.Device.Status.ToString(),
                CellId = c.Id,
                CellLabel = c.CellLabel,
                MedicationId = c.MedicationId!.Value,
                MedicationName = c.Medication!.Name,
                Quantity = c.CurrentQuantity
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<CellDto>> GetDeviceInventory(int deviceId)
    {
        List<CellDto> cells = await dataContext.Cells
            .Where(c => c.DeviceId == deviceId)
            .Select(c => new CellDto
            {
                Id = c.Id,
                Label = c.CellLabel,
                MedicationName = c.Medication != null ? c.Medication.Name : string.Empty,
                Quantity = c.CurrentQuantity
            })
            .ToListAsync();

        return cells;
    }

    public async Task RefillCell(Application.DTOs.Inventory.UpdateCellDto dto)
    {
        var cell = await dataContext.Cells.FindAsync(dto.CellId);

        if (cell == null)
            throw new NotFoundException($"Cell not found with id: ({dto.CellId})");

        if (string.IsNullOrWhiteSpace(dto.Label))
            throw new ValidationException("Cell label is required");

        if (dto.Quantity < 0)
            throw new ValidationException("Quantity cannot be negative");

        if (!await dataContext.Medications.AnyAsync(m => m.Id == dto.MedicationId))
            throw new NotFoundException($"Medication {dto.MedicationId} does not exist");

        var duplicateLabelExists = await dataContext.Cells
            .AnyAsync(c => c.DeviceId == cell.DeviceId &&
                           c.Id != cell.Id &&
                           c.CellLabel == dto.Label);

        if (duplicateLabelExists)
            throw new ValidationException($"Cell with label '{dto.Label}' already exists in this device");

        cell.CellLabel = dto.Label;
        cell.CurrentQuantity = dto.Quantity;
        cell.MedicationId = dto.MedicationId;

        await dataContext.SaveChangesAsync(CancellationToken.None);
    }
}
