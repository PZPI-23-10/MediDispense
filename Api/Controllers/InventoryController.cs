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
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet("device/{deviceId:int}")]
    public async Task<ActionResult<IEnumerable<CellDto>>> GetDeviceInventory(int deviceId)
    {
        IEnumerable<CellDto> cells = await inventoryService.GetDeviceInventory(deviceId);

        return Ok(cells);
    }

    [HttpPut("update")]
    public async Task<IActionResult> RefillCell([FromBody] UpdateCellDto dto)
    {
        await inventoryService.RefillCell(dto);
        return Ok();
    }
}