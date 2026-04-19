namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для Rating (0-10).
/// </summary>
public class RatingValidator : AbstractValidator<Rating>
{
    public RatingValidator()
    {
        RuleFor(x => x.BookRating)
            .InclusiveBetween(0, 10).When(x => x.BookRating.HasValue)
            .WithMessage("Rating must be between 0 and 10");

        RuleFor(x => x.AdaptationRating)
            .InclusiveBetween(0, 10).When(x => x.AdaptationRating.HasValue)
            .WithMessage("Rating must be between 0 and 10");
    }
}
