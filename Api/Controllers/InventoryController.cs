using Application.DTOs.Inventory;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CellDto = Application.DTOs.Device.CellDto;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin},{UserRoles.Doctor}")]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<AvailableMedicationDto>>> GetAvailableMedications()
    {
        IEnumerable<AvailableMedicationDto> items = await inventoryService.GetAvailableMedications();

        return Ok(items);
    }

    [HttpGet("device/{deviceId:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<IEnumerable<CellDto>>> GetDeviceInventory(int deviceId)
    {
        IEnumerable<CellDto> cells = await inventoryService.GetDeviceInventory(deviceId);

        return Ok(cells);
    }

    [HttpPut("update")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> RefillCell([FromBody] UpdateCellDto dto)
    {
        await inventoryService.RefillCell(dto);
        return Ok();
    }
}
