namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Adaptation.
/// </summary>
public class AdaptationValidator : AbstractValidator<Adaptation>
{
    public AdaptationValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255);

        RuleFor(x => x.Type)
            .Must(type => new[] { "movie", "series" }.Contains(type.ToLower()))
            .WithMessage("Type must be either 'movie' or 'series'");

        RuleFor(x => x.ReleaseYear)
            .GreaterThan(0).When(x => x.ReleaseYear.HasValue)
            .WithMessage("Release year must be greater than 0");
    }
}
