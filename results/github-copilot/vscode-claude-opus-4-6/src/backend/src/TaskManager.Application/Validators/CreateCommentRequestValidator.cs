// FluentValidation validator for create comment requests.
using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required")
            .MaximumLength(2000).WithMessage("Comment body must not exceed 2000 characters");
    }
}
