// <copyright file="DifferenceValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для конкретної відмінності (Difference).
/// </summary>
public class DifferenceValidator : AbstractValidator<Difference>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DifferenceValidator"/> class.
    /// </summary>
    public DifferenceValidator()
    {
        this.RuleFor(x => x.Title).NotEmpty();
        this.RuleFor(x => x.BookText).NotEmpty();
        this.RuleFor(x => x.FilmText).NotEmpty();

        this.RuleFor(x => x.ImportanceLevel)
            .Must(l => new[] { "low", "medium", "high" }.Contains(l.ToLower()));
    }
}
