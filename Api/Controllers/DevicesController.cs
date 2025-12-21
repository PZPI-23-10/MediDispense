using Application.DTOs.Device;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService deviceService) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> RegisterDevice([FromBody] CreateDeviceDto dto)
    {
        DeviceDetailsDto device = await deviceService.Add(dto);

        return CreatedAtAction(nameof(Get), new { id = device.Id }, device);
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] int deviceId)
    {
        await deviceService.Heartbeat(deviceId);

        return Ok();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<IEnumerable<DeviceDetailsDto>>> GetAll()
    {
        IEnumerable<DeviceDetailsDto> devices = await deviceService.GetAll();
        return Ok(devices);
    }

    [HttpGet]
    [Route("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<DeviceDetailsDto>> Get(int id)
    {
        DeviceDetailsDto device = await deviceService.GetById(id);

        return Ok(device);
    }

    [HttpPost("{deviceId}/cells")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> AddCell(int deviceId, [FromBody] CreateCellDto dto)
    {
        CreateCellResult cellResult = await deviceService.CreateCell(deviceId, dto);
        return Ok(cellResult);
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        await deviceService.Delete(id);

        return NoContent();
    }
}