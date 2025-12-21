using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class LoginUserRequest
{
    [Required] public string Username { get; set; }
    [Required] public string Password { get; set; }
}