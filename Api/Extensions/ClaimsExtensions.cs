using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.Exceptions;

namespace Api.Extensions;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedException("User ID claim not found.");

        if (!int.TryParse(claim.Value, out int userId))
            throw new ValidationException("User ID claim is not a valid integer.");

        return userId;
    }
}