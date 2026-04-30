// TaskValidators.cs
// FluentValidation rules for create/update task requests.
using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validation;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
    }
}
