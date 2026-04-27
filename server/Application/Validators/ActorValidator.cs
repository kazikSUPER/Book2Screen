// <copyright file="ActorValidator.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Validators;

using Book2Screen.Domain.Entities;
using FluentValidation;

/// <summary>
/// Валідатор для сутності Actor.
/// </summary>
public class ActorValidator : AbstractValidator<Actor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActorValidator"/> class.
    /// </summary>
    public ActorValidator()
    {
        this.RuleFor(x => x.FullName)
            .NotEmpty().MaximumLength(150);
    }
}
