using Application.DTOs.Dispense;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DispenseController(IDispenseService dispenseService) : ControllerBase
{
    [HttpPost("dispense")]
    public async Task<ActionResult<DispenseInstructionDto>> Verify([FromBody] VerifyPrescriptionRequest request)
    {
        var result = await dispenseService.Dispense(request);
        return Ok(result);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmDispenseRequest request)
    {
        await dispenseService.ConfirmDispense(request);
        return Ok();
    }
}