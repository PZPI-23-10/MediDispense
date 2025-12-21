using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface ITokenService
{
    Token GenerateAccessToken(string id, string email, IEnumerable<string> roles, bool rememberMe);
    DateTime GetTokenExpirationTime(string token);
}