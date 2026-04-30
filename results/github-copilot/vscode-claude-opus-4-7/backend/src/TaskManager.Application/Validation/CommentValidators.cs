// CommentValidators.cs
// FluentValidation rules for comment create/update DTOs.
using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validation;

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
