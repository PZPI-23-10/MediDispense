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
    public async Task<IEnumerable<CellDto>> GetDeviceInventory(int deviceId)
    {
        List<CellDto> cells = await dataContext.Cells
            .Where(c => c.DeviceId == deviceId)
            .Select(c => new CellDto
            {
                Id = c.Id,
                Label = c.CellLabel,
                MedicationName = c.Medication.Name,
                Quantity = c.CurrentQuantity
            })
            .ToListAsync();

        return cells;
    }

    public async Task RefillCell(UpdateCellDto dto)
    {
        var cell = await dataContext.Cells.FindAsync(dto.CellId);

        if (cell == null)
            throw new NotFoundException($"Cell not found with id: ({dto.CellId})");

        if (dto.Quantity < 0)
            throw new ValidationException("Quantity cannot be negative");

        cell.CellLabel = dto.Label;
        cell.CurrentQuantity = dto.Quantity;
        cell.MedicationId = dto.MedicationId;

        await dataContext.SaveChangesAsync(CancellationToken.None);
    }
}