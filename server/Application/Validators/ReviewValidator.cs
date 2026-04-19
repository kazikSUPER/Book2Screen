namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Review.
/// </summary>
public class ReviewValidator : AbstractValidator<Review>
{
    public ReviewValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().MinimumLength(10).WithMessage("Review must be at least 10 characters long");

        RuleFor(x => x.TargetType)
            .Must(t => new[] { "book", "adaptation", "comparison" }.Contains(t.ToLower()))
            .WithMessage("Target type must be: book, adaptation or comparison");

        RuleFor(x => x.WorkId).NotEmpty();
    }
}
