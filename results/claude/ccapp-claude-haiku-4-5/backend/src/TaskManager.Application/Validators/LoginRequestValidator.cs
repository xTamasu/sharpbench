using FluentValidation;
using TaskManager.Application.Dto.Auth;

namespace TaskManager.Application.Validators;

/// <summary>
/// Validator for user login.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
