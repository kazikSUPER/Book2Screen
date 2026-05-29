// <copyright file="ForbiddenException.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Exceptions;

/// <summary>
/// Виключення, що виникає, коли у користувача недостатньо прав для доступу до ресурсу.
/// </summary>
public class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">Повідомлення про помилку.</param>
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
