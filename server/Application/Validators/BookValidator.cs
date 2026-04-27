// <copyright file="BookValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Book.
/// </summary>
public class BookValidator : AbstractValidator<Book>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookValidator"/> class.
    /// </summary>
    public BookValidator()
    {
        this.RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255);

        this.RuleFor(x => x.PublicationYear)
            .GreaterThan(0).When(x => x.PublicationYear.HasValue)
            .WithMessage("Publication year must be greater than 0");
    }
}
