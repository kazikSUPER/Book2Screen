// <copyright file="PlotEventValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для PlotEvent (Послідовність подій).
/// </summary>
public class PlotEventValidator : AbstractValidator<PlotEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlotEventValidator"/> class.
    /// </summary>
    public PlotEventValidator()
    {
        this.RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(255);

        this.RuleFor(x => x.SequenceNumber)
            .GreaterThan(0).WithMessage("Sequence number must be positive");

        this.RuleFor(x => x.SourceType)
            .Must(s => new[] { "book", "adaptation" }.Contains(s.ToLower()))
            .WithMessage("Source must be 'book' or 'adaptation'");
    }
}
