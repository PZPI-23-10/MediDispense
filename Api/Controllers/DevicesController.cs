using Application.DTOs.Device;
using Application.Exceptions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CellDto = Application.DTOs.Device.CellDto;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDataContext dataContext) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> RegisterDevice([FromBody] CreateDeviceDto dto)
    {
        var device = new Device
        {
            Title = dto.Title,
            Status = DeviceStatus.Offline,
        };

        dataContext.Devices.Add(device);
        await dataContext.SaveChangesAsync(CancellationToken.None);

        return CreatedAtAction(nameof(GetAll), new { id = device.Id }, new { device.Id, device.Title });
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] int deviceId)
    {
        var device = await dataContext.Devices.FindAsync(deviceId);
        if (device == null) return NotFound();

        device.LastActive = DateTime.UtcNow;

        if (device.Status == DeviceStatus.Offline)
        {
            device.Status = DeviceStatus.Online;
        }

        await dataContext.SaveChangesAsync(CancellationToken.None);
        return Ok();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<List<DeviceDetailsDto>>> GetAll()
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

        return Ok(devices);
    }

    [HttpGet]
    [Route("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<DeviceDetailsDto>> Get(int id)
    {
        var device = await dataContext.Devices.FindAsync(id);

        if (device == null)
            return NotFound();

        return Ok(new DeviceDetailsDto
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
        });
    }

    [HttpPost("{deviceId}/cells")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> AddCell(int deviceId, [FromBody] CreateCellDto dto)
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

        return Ok(new { cell.Id, cell.CellLabel });
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var device = await dataContext.Devices.FindAsync(id);

        if (device == null)
            throw new NotFoundException($"Device with id {id} not found");

        dataContext.Devices.Remove(device);
        await dataContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task SetDeviceStatus(int deviceId, DeviceStatus newStatus)
    {
        var device = await dataContext.Devices.FindAsync(deviceId);

        if (device == null)
            throw new NotFoundException($"Device with id: {deviceId} not found");

        device.Status = newStatus;
        await dataContext.SaveChangesAsync(CancellationToken.None);
    }
}