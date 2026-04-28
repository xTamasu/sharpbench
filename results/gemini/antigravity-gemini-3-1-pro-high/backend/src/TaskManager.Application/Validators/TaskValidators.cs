using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators
{
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(5000);
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

    public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
    {
        public CreateCommentRequestValidator()
        {
            RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        }
    }

    public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
    {
        public UpdateCommentRequestValidator()
        {
            RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        }
    }
}
