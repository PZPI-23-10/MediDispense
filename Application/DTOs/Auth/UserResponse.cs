namespace Application.DTOs.Auth;

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public IEnumerable<string> Roles { get; set; }
}