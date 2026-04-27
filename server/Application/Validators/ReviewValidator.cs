// <copyright file="ReviewValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Review.
/// </summary>
public class ReviewValidator : AbstractValidator<Review>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewValidator"/> class.
    /// </summary>
    public ReviewValidator()
    {
        this.RuleFor(x => x.Text)
            .NotEmpty().MinimumLength(10).WithMessage("Review must be at least 10 characters long");

        this.RuleFor(x => x.TargetType)
            .Must(t => new[] { "book", "adaptation", "comparison" }.Contains(t.ToLower()))
            .WithMessage("Target type must be: book, adaptation or comparison");

        this.RuleFor(x => x.WorkId).NotEmpty();
    }
}
