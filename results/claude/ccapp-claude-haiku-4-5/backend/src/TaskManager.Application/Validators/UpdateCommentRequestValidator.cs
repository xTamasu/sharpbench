using FluentValidation;
using TaskManager.Application.Dto.Comment;

namespace TaskManager.Application.Validators;

/// <summary>
/// Validator for comment updates.
/// </summary>
public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Comment body is required.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}
