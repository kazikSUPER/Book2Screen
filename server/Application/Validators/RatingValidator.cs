// <copyright file="RatingValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для Rating (0-10).
/// </summary>
public class RatingValidator : AbstractValidator<Rating>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RatingValidator"/> class.
    /// </summary>
    public RatingValidator()
    {
        this.RuleFor(x => x.BookRating)
            .InclusiveBetween(0, 10).When(x => x.BookRating.HasValue)
            .WithMessage("Rating must be between 0 and 10");

        this.RuleFor(x => x.AdaptationRating)
            .InclusiveBetween(0, 10).When(x => x.AdaptationRating.HasValue)
            .WithMessage("Rating must be between 0 and 10");
    }
}
