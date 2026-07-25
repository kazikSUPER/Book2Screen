// <copyright file="ReviewRequestValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Application.DTOs;
using FluentValidation;

/// <summary>
/// Валідатор для запиту на створення відгуку (ReviewRequest).
/// </summary>
public class ReviewRequestValidator : AbstractValidator<ReviewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewRequestValidator"/> class.
    /// </summary>
    public ReviewRequestValidator()
    {
        this.RuleFor(x => x.WorkId)
            .NotEmpty().WithMessage("Work ID is required.");

        this.RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Review text cannot be empty.")
            .MinimumLength(10).WithMessage("Review must be at least 10 characters long.")
            .MaximumLength(2000).WithMessage("Review text cannot exceed 2000 characters.");

        this.RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).WithMessage("Rating must be between 0 and 10.");

        this.RuleFor(x => x.TargetType)
            .Must(t => new[] { "book", "adaptation", "comparison" }.Contains(t.ToLower()))
            .WithMessage("Target type must be: book, adaptation, or comparison.");
    }
}
