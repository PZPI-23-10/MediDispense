using System.ComponentModel.DataAnnotations;
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

        device.LastActive = DateTimeOffset.UtcNow;

        if (device.Status == DeviceStatus.Offline)
            device.Status = DeviceStatus.Online;

        await dataContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task<IEnumerable<DeviceDetailsDto>> GetAll()
    {
        return await dataContext.Devices
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
        ValidateDeviceTitle(dto.Title);

        var device = new Device
        {
            Title = dto.Title,
            Status = DeviceStatus.Offline,
        };

        dataContext.Devices.Add(device);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return await GetById(device.Id);
    }

    public async Task<DeviceDetailsDto> Update(int deviceId, UpdateDeviceDto dto)
    {
        ValidateDeviceTitle(dto.Title);

        var device = await dataContext.Devices.FindAsync(deviceId);

        if (device == null)
            throw new NotFoundException($"Device with id {deviceId} not found");

        device.Title = dto.Title;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse(dto.Status, true, out DeviceStatus status))
                throw new ValidationException($"Device status '{dto.Status}' is invalid");

            device.Status = status;
        }

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

    public async Task<IEnumerable<CellDto>> GetCells(int deviceId)
    {
        await EnsureDeviceExists(deviceId);

        return await dataContext.Cells
            .Where(c => c.DeviceId == deviceId)
            .Select(c => new CellDto
            {
                Id = c.Id,
                Label = c.CellLabel,
                MedicationName = c.Medication != null ? c.Medication.Name : string.Empty,
                Quantity = c.CurrentQuantity
            })
            .ToListAsync();
    }

    public async Task<CreateCellResult> CreateCell(int deviceId, CreateCellDto dto)
    {
        ValidateCellLabel(dto.Label);

        if (dto.InitialQuantity < 0)
            throw new ValidationException("Initial quantity cannot be negative");

        await EnsureDeviceExists(deviceId);

        if (dto.MedicationId.HasValue)
            await EnsureMedicationExists(dto.MedicationId.Value);

        await EnsureCellLabelIsUnique(deviceId, dto.Label);

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

    public async Task<CellDto> UpdateCell(int deviceId, int cellId, UpdateCellDto dto)
    {
        ValidateCellLabel(dto.Label);

        if (dto.Quantity < 0)
            throw new ValidationException("Quantity cannot be negative");

        var cell = await dataContext.Cells.FindAsync(cellId);

        if (cell == null)
            throw new NotFoundException($"Cell with id {cellId} not found");

        if (cell.DeviceId != deviceId)
            throw new ValidationException($"Cell with id {cellId} does not belong to device {deviceId}");

        if (dto.MedicationId.HasValue)
            await EnsureMedicationExists(dto.MedicationId.Value);

        await EnsureCellLabelIsUnique(deviceId, dto.Label, cellId);

        cell.CellLabel = dto.Label;
        cell.MedicationId = dto.MedicationId;
        cell.CurrentQuantity = dto.Quantity;

        await dataContext.SaveChangesAsync(CancellationToken.None);

        return await dataContext.Cells
            .Where(c => c.Id == cellId)
            .Select(c => new CellDto
            {
                Id = c.Id,
                Label = c.CellLabel,
                MedicationName = c.Medication != null ? c.Medication.Name : string.Empty,
                Quantity = c.CurrentQuantity
            })
            .SingleAsync();
    }

    public async Task DeleteCell(int deviceId, int cellId)
    {
        var cell = await dataContext.Cells.FindAsync(cellId);

        if (cell == null)
            throw new NotFoundException($"Cell with id {cellId} not found");

        if (cell.DeviceId != deviceId)
            throw new ValidationException($"Cell with id {cellId} does not belong to device {deviceId}");

        dataContext.Cells.Remove(cell);
        await dataContext.SaveChangesAsync(CancellationToken.None);
    }

    private static void ValidateDeviceTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Device title is required");
    }

    private static void ValidateCellLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ValidationException("Cell label is required");
    }

    private async Task EnsureDeviceExists(int deviceId)
    {
        if (!await dataContext.Devices.AnyAsync(d => d.Id == deviceId))
            throw new NotFoundException($"Device with id {deviceId} not found");
    }

    private async Task EnsureMedicationExists(int medicationId)
    {
        if (!await dataContext.Medications.AnyAsync(m => m.Id == medicationId))
            throw new NotFoundException($"Medication {medicationId} does not exist");
    }

    private async Task EnsureCellLabelIsUnique(int deviceId, string label, int? ignoredCellId = null)
    {
        var exists = await dataContext.Cells
            .AnyAsync(c => c.DeviceId == deviceId &&
                           c.CellLabel == label &&
                           (!ignoredCellId.HasValue || c.Id != ignoredCellId.Value));

        if (exists)
            throw new ValidationException($"Cell with label '{label}' already exists in this device");
    }
}
