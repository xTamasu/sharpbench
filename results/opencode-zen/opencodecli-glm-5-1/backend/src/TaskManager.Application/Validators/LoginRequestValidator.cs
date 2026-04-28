// FluentValidation validator for login requests
using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}