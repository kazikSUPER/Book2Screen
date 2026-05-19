// <copyright file="ConflictException.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Exceptions;

/// <summary>
/// Виключення, що виникає при конфлікті даних (наприклад, дублікат email).
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">Повідомлення про помилку.</param>
    public ConflictException(string message)
        : base(message)
    {
    }
}
