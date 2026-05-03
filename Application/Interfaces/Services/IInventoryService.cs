using Application.DTOs.Device;
using Application.DTOs.Inventory;

namespace Application.Interfaces.Services;

public interface IInventoryService
{
    Task<IEnumerable<AvailableMedicationDto>> GetAvailableMedications();
    Task<IEnumerable<CellDto>> GetDeviceInventory(int deviceId);
    Task RefillCell(UpdateCellDto dto);
}
