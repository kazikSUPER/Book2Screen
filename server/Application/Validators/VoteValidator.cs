// <copyright file="VoteValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для Vote.
/// </summary>
public class VoteValidator : AbstractValidator<Vote>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VoteValidator"/> class.
    /// </summary>
    public VoteValidator()
    {
        this.RuleFor(x => x.SelectedOption)
            .Must(o => new[] { "book", "adaptation" }.Contains(o.ToLower()))
            .WithMessage("Vote for either 'book' or 'adaptation'");

        this.RuleFor(x => x.WorkId).NotEmpty();
    }
}
