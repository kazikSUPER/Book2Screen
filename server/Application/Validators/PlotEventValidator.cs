namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для PlotEvent (Послідовність подій).
/// </summary>
public class PlotEventValidator : AbstractValidator<PlotEvent>
{
    public PlotEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(255);

        RuleFor(x => x.SequenceNumber)
            .GreaterThan(0).WithMessage("Sequence number must be positive");

        RuleFor(x => x.SourceType)
            .Must(s => new[] { "book", "adaptation" }.Contains(s.ToLower()))
            .WithMessage("Source must be 'book' or 'adaptation'");
    }
}
