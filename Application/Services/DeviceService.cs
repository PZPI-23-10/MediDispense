using Application.DTOs.Device;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DeviceService(IDataContext dataContext) : IDeviceService
{
    public async Task Heartbeat(int deviceId)
    {
        var device = await dataContext.Devices.FindAsync(deviceId);

        if (device == null)
            throw new NotFoundException("Device not found");

        device.LastActive = DateTime.UtcNow;

        if (device.Status == DeviceStatus.Offline)
            device.Status = DeviceStatus.Online;

        await dataContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task<IEnumerable<DeviceDetailsDto>> GetAll()
    {
        var devices = await dataContext.Devices
            .Select(d => new DeviceDetailsDto
            {
                Id = d.Id,
                Title = d.Title,
                Status = d.Status.ToString(),
                Cells = d.Cells.Select(c => new CellDto
                {
                    Id = c.Id,
                    Label = c.CellLabel,
                    MedicationName = c.Medication != null ? c.Medication.Name : string.Empty,
                    Quantity = c.CurrentQuantity
                }).ToList()
            })
            .ToListAsync();

        return devices;
    }

    public async Task<DeviceDetailsDto> GetById(int deviceId)
    {
        var device = await dataContext.Devices.FindAsync(deviceId);

        if (device == null)
            throw new NotFoundException("Device not found");

        return new DeviceDetailsDto
        {
            Id = device.Id,
            Title = device.Title,
            Status = device.Status.ToString(),
            Cells = device.Cells.Select(c => new CellDto
            {
                Id = c.Id,
                Label = c.CellLabel,
                MedicationName = c.Medication != null ? c.Medication.Name : string.Empty,
                Quantity = c.CurrentQuantity
            }).ToList()
        };
    }

    public async Task<DeviceDetailsDto> Add(CreateDeviceDto dto)
    {
        var device = new Device
        {
            Title = dto.Title,
            Status = DeviceStatus.Offline,
        };

        dataContext.Devices.Add(device);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return await GetById(device.Id);
    }

    public async Task Delete(int deviceId)
    {
        var device = await dataContext.Devices.FindAsync(deviceId);

        if (device == null)
            throw new NotFoundException($"Device with id {deviceId} not found");

        dataContext.Devices.Remove(device);
        await dataContext.SaveChangesAsync();
    }

    public async Task<CreateCellResult> CreateCell(int deviceId, CreateCellDto dto)
    {
        var device = await dataContext.Devices.FindAsync(deviceId);
        if (device == null)
            throw new NotFoundException($"Device with ID {deviceId} not found");

        var medicationExists = await dataContext.Medications.AnyAsync(m => m.Id == dto.MedicationId);
        if (!medicationExists)
            throw new NotFoundException($"Medication {dto.MedicationId} does not exist");

        var cellExists = await dataContext.Cells
            .AnyAsync(c => c.DeviceId == deviceId && c.CellLabel == dto.Label);

        if (cellExists)
            throw new InvalidOperationException($"Cell with label '{dto.Label}' already exists in this device.");

        var cell = new Cell
        {
            DeviceId = deviceId,
            CellLabel = dto.Label,
            MedicationId = dto.MedicationId,
            CurrentQuantity = dto.InitialQuantity ?? 0
        };

        dataContext.Cells.Add(cell);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return new CreateCellResult { Id = cell.Id, Label = cell.CellLabel };
    }
}