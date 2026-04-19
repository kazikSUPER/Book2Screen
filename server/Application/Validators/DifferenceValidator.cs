namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для конкретної відмінності (Difference).
/// </summary>
public class DifferenceValidator : AbstractValidator<Difference>
{
    public DifferenceValidator()
    {
        RuleFor(x => x.Description).NotEmpty();

        RuleFor(x => x.DifferenceType)
            .Must(t => new[] { "changed", "added", "removed" }.Contains(t.ToLower()));

        RuleFor(x => x.ImportanceLevel)
            .Must(l => new[] { "low", "medium", "high" }.Contains(l.ToLower()));
    }
}

