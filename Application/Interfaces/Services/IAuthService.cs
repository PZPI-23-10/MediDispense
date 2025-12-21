using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    Task<RegisterUserResponse> Register(RegisterUserRequest request);
    Task<LoginUserResponse> Login(LoginUserRequest request);

    Task<UserResponse> GetUserById(int id);
    Task<IEnumerable<UserResponse>> GetAllUsers();
    Task Delete(int userId);
}