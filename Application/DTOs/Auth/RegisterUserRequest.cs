using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterUserRequest
{
    [Required] public string Username { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string FullName { get; set; }
    [Required] public string Password { get; set; }
    [Required] public string Role { get; set; }
}