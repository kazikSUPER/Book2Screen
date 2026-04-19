namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Book.
/// </summary>
public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255);

        RuleFor(x => x.PublicationYear)
            .GreaterThan(0).When(x => x.PublicationYear.HasValue)
            .WithMessage("Publication year must be greater than 0");
    }
}
