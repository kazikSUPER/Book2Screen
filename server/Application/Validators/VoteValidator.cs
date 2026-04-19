namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для Vote.
/// </summary>
public class VoteValidator : AbstractValidator<Vote>
{
    public VoteValidator()
    {
        RuleFor(x => x.SelectedOption)
            .Must(o => new[] { "book", "adaptation" }.Contains(o.ToLower()))
            .WithMessage("Vote for either 'book' or 'adaptation'");

        RuleFor(x => x.WorkId).NotEmpty();
    }
}
