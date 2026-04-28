// <copyright file="AuthorValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Author.
/// </summary>
public class AuthorValidator : AbstractValidator<Author>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorValidator"/> class.
    /// </summary>
    public AuthorValidator()
    {
        this.RuleFor(x => x.FullName)
            .NotEmpty().MaximumLength(150);

        this.RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow).When(x => x.BirthDate.HasValue)
            .WithMessage("Birth date cannot be in the future");
    }
}
