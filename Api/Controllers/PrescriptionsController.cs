using Api.Extensions;
using Application.DTOs.Prescription;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{UserRoles.Admin},{UserRoles.Doctor}")]
public class PrescriptionsController(IPrescriptionService prescriptionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreatePrescriptionResponse>> Create(CreatePrescriptionRequest request)
    {
        var userId = User.GetUserId();

        var prescription = await prescriptionService.Create(userId, User.IsInRole(UserRoles.Admin), request);

        return Ok(prescription);
    }

    [HttpGet]
    [Route("qr/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQrCode(int id)
    {
        byte[] qrCode = await prescriptionService.GetQrCode(id);

        return File(qrCode, "image/jpeg");
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<PrescriptionResponseDto>> Get(int id)
    {
        var prescription = await prescriptionService.Get(id, User.GetUserId(), User.IsInRole(UserRoles.Admin));

        return Ok(prescription);
    }

    [HttpGet]
    public async Task<ActionResult<List<PrescriptionResponseDto>>> GetAll(
        [FromQuery] int? patientId,
        [FromQuery] int? doctorId,
        [FromQuery] int? status)
    {
        var currentUserId = User.GetUserId();

        IEnumerable<PrescriptionResponseDto> prescriptions =
            await prescriptionService.GetAll(patientId, doctorId, status, currentUserId, User.IsInRole(UserRoles.Admin));

        return Ok(prescriptions);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PrescriptionResponseDto>> Update(int id, [FromBody] UpdatePrescriptionRequest request)
    {
        PrescriptionResponseDto prescription =
            await prescriptionService.Update(id, User.GetUserId(), User.IsInRole(UserRoles.Admin), request);

        return Ok(prescription);
    }

    [HttpPost]
    [Route("cancel")]
    public async Task<IActionResult> Cancel([FromBody] int id)
    {
        await prescriptionService.Cancel(id, User.GetUserId(), User.IsInRole(UserRoles.Admin));

        return Ok();
    }
}
