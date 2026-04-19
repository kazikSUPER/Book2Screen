namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Work (Зв'язок Книга-Екранізація).
/// </summary>
public class WorkValidator : AbstractValidator<Work>
{
    public WorkValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(255);

        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.AdaptationId).NotEmpty();
    }
}
