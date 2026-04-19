namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для DifferenceMap.
/// </summary>
public class DifferenceMapValidator : AbstractValidator<DifferenceMap>
{
    public DifferenceMapValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
