// FluentValidation validators for all request DTOs.

using FluentValidation;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.DTOs.Comments;

namespace TaskManager.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(100).WithMessage("Display name must be at most 100 characters");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters");
        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must be at most 5000 characters");
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value");
        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");
    }
}

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters");
        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must be at most 5000 characters");
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value");
        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");
    }
}

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required")
            .MaximumLength(2000).WithMessage("Comment body must be at most 2000 characters");
    }
}

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required")
            .MaximumLength(2000).WithMessage("Comment body must be at most 2000 characters");
    }
}
