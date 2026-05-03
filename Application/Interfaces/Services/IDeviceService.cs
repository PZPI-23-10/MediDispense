using Application.DTOs.Device;

namespace Application.Interfaces.Services;

public interface IDeviceService
{
    Task Heartbeat(int deviceId);
    Task<IEnumerable<DeviceDetailsDto>> GetAll();
    Task<DeviceDetailsDto> GetById(int deviceId);
    Task<DeviceDetailsDto> Add(CreateDeviceDto dto);
    Task<DeviceDetailsDto> Update(int deviceId, UpdateDeviceDto dto);
    Task Delete(int deviceId);
    Task<IEnumerable<CellDto>> GetCells(int deviceId);
    Task<CreateCellResult> CreateCell(int deviceId, CreateCellDto dto);
    Task<CellDto> UpdateCell(int deviceId, int cellId, UpdateCellDto dto);
    Task DeleteCell(int deviceId, int cellId);
}
