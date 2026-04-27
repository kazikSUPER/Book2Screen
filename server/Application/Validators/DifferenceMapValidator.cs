// <copyright file="DifferenceMapValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для DifferenceMap.
/// </summary>
public class DifferenceMapValidator : AbstractValidator<DifferenceMap>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DifferenceMapValidator"/> class.
    /// </summary>
    public DifferenceMapValidator()
    {
        this.RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        this.RuleFor(x => x.Version).GreaterThan(0);
    }
}
