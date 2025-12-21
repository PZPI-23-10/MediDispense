using Application.DTOs.Device;

namespace Application.Interfaces.Services;

public interface IDeviceService
{
    Task Heartbeat(int deviceId);
    Task<IEnumerable<DeviceDetailsDto>> GetAll();
    Task<DeviceDetailsDto> GetById(int deviceId);
    Task<DeviceDetailsDto> Add(CreateDeviceDto dto);
    Task Delete(int deviceId);
    Task<CreateCellResult> CreateCell(int deviceId, CreateCellDto dto);
}