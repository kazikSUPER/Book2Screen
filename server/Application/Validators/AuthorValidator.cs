namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Author.
/// </summary>
public class AuthorValidator : AbstractValidator<Author>
{
    public AuthorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().MaximumLength(150);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow).When(x => x.BirthDate.HasValue)
            .WithMessage("Birth date cannot be in the future");
    }
}
