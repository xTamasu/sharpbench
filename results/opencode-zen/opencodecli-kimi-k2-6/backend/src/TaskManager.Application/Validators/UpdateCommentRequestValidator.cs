using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

/// <summary>
/// Validator for comment update requests.
/// </summary>
public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required.")
            .MaximumLength(2000).WithMessage("Comment body must not exceed 2000 characters.");
    }
}
