// <copyright file="EmailOptions.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Configurations;

/// <summary>
/// Налаштування для SMTP сервера.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Назва секції в конфігурації.
    /// </summary>
    public const string EmailSettings = "EmailSettings";

    /// <summary>
    /// Gets or sets хост SMTP сервера.
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets порт SMTP сервера.
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// Gets or sets пошту відправника.
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets пароль (або токен додатку) відправника.
    /// </summary>
    public string SenderPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets ім'я відправника.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
}
