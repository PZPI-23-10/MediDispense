using System.ComponentModel.DataAnnotations;
using Application.DTOs.Inventory;
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
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
public class InventoryController(IDataContext dataContext) : ControllerBase
{
    [HttpGet("device/{deviceId:int}")]
    public async Task<ActionResult<IEnumerable<CellDto>>> GetDeviceInventory(int deviceId)
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

        return Ok(cells);
    }

    [HttpPut("update")]
    public async Task<IActionResult> RefillCell([FromBody] UpdateCellDto dto)
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
        return Ok();
    }
}