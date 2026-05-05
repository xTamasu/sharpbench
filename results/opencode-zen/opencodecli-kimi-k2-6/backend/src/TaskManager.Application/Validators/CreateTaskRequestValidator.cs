using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

/// <summary>
/// Validator for task creation requests.
/// </summary>
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority.");
    }
}
