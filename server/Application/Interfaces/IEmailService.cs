// <copyright file="IEmailService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Interfaces;

/// <summary>
/// Інтерфейс для сервісу відправки електронної пошти.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Відправляє електронне повідомлення.
    /// </summary>
    /// <param name="to">Адреса отримувача.</param>
    /// <param name="subject">Тема листа.</param>
    /// <param name="body">Текст листа (HTML).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body);
}
