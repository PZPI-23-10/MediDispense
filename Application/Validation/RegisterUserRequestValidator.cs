using Application.DTOs.Auth;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Validation;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    private readonly UserManager<User> _users;

    public RegisterUserRequestValidator(UserManager<User> users)
    {
        _users = users;

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MustAsync(BeUniqueEmail)
            .WithMessage("User with this email already exists.")
            .MustAsync(BeUniqueUsername)
            .WithMessage("User with this username already exists.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return await _users.FindByEmailAsync(email) == null;
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
        return await _users.FindByNameAsync(username) == null;
    }
}