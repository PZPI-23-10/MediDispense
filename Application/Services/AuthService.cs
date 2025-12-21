using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService(ITokenService tokenService, UserManager<User> userManager) : IAuthService
{
    public async Task<RegisterUserResponse> Register(RegisterUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            UserName = request.Username,
        };

        if (request.Role != UserRoles.Admin && request.Role != UserRoles.Doctor)
            throw new ValidationException($"User role {request.Role} does not exist");

        IdentityResult registerResult = await userManager.CreateAsync(user, request.Password);

        if (!registerResult.Succeeded)
            throw new InvalidOperationException(
                $"Register failed with errors: {string.Join(',', registerResult.Errors.Select(e => e.Code))}");

        IdentityResult roleResult = await userManager.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
            throw new InvalidOperationException(
                $"Could not add role to user with errors: {string.Join(',', roleResult.Errors.Select(e => e.Code))}");

        return new RegisterUserResponse
        {
            UserId = user.Id
        };
    }

    public async Task<LoginUserResponse> Login(LoginUserRequest request)
    {
        User? user = await userManager.FindByNameAsync(request.Username);

        if (user == null)
            throw new NotFoundException("User with this username does not exist.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid Password.");

        IList<string> roles = await userManager.GetRolesAsync(user);
        Token accessToken = tokenService.GenerateAccessToken(user.Id.ToString(), user.Email, roles, true);

        return new LoginUserResponse
        {
            UserId = user.Id,
            Token = accessToken.TokenKey
        };
    }

    public async Task<UserResponse> GetUserById(int id)
    {
        User? user = await userManager.FindByIdAsync(id.ToString());

        if (user == null)
            throw new NotFoundException("User with this id does not exist.");

        IList<string> roles = await userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Username = user.UserName,
            Roles = roles
        };
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsers()
    {
        List<User> users = await userManager.Users.ToListAsync();

        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);

            result.Add(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Username = user.UserName,
                Roles = roles
            });
        }

        return result;
    }

    public async Task Delete(int userId)
    {
        User? user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            throw new NotFoundException("User with this id does not exist.");

        await userManager.DeleteAsync(user);
    }
}