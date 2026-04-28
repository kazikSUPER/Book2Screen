// <copyright file="AdaptationValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Adaptation.
/// </summary>
public class AdaptationValidator : AbstractValidator<Adaptation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptationValidator"/> class.
    /// </summary>
    public AdaptationValidator()
    {
        this.RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255);

        this.RuleFor(x => x.Type)
            .Must(type => new[] { "movie", "series" }.Contains(type.ToLower()))
            .WithMessage("Type must be either 'movie' or 'series'");

        this.RuleFor(x => x.ReleaseYear)
            .GreaterThan(0).When(x => x.ReleaseYear.HasValue)
            .WithMessage("Release year must be greater than 0");
    }
}
