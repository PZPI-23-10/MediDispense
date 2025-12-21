using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<ActionResult<LoginUserResponse>> RegisterUser(
        [FromBody] RegisterUserRequest request,
        [FromServices] IValidator<RegisterUserRequest> validator
    )
    {
        ValidationResult? validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
        }

        RegisterUserResponse result = await authService.Register(request);

        return Ok(result);
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<LoginUserResponse>> LoginUser([FromBody] LoginUserRequest request)
    {
        LoginUserResponse result = await authService.Login(request);

        return Ok(result);
    }

    [HttpGet]
    [Route("{userId:int}")]
    public async Task<ActionResult<UserResponse>> GetUser(int userId)
    {
        UserResponse result = await authService.GetUserById(userId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        var result = await authService.GetAllUsers();
        return Ok(result);
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = UserRoles.Admin)]
    public async Task<IActionResult> DeleteUser([FromBody] int userId)
    {
        await authService.Delete(userId);

        return Ok();
    }
}