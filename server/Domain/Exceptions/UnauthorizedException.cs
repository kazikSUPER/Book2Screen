// <copyright file="UnauthorizedException.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Exceptions;

/// <summary>
/// Виключення, що виникає при помилці авторизації (наприклад, невірні облікові дані).
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">Повідомлення про помилку.</param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
