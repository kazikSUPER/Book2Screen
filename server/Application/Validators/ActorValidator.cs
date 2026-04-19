namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Actor.
/// </summary>
public class ActorValidator : AbstractValidator<Actor>
{
    public ActorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().MaximumLength(150);
    }
}
