// <copyright file="WorkValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Work (Зв'язок Книга-Екранізація).
/// </summary>
public class WorkValidator : AbstractValidator<Work>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkValidator"/> class.
    /// </summary>
    public WorkValidator()
    {
        this.RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(255);

        this.RuleFor(x => x.BookId).NotEmpty();
        this.RuleFor(x => x.AdaptationId).NotEmpty();
    }
}
