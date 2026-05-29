// <copyright file="ReportResponse.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Відповідь зі скаргою на відгук.
/// </summary>
public class ReportResponse
{
    /// <summary>
    /// Gets or sets iD скарги.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Gets or sets iD відгуку.
    /// </summary>
    public Guid ReviewId { get; set; }

    /// <summary>
    /// Gets or sets iD користувача, який подав скаргу.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets причину скарги.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Gets or sets статус скарги.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets дату створення.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets текст відгуку (для зручності модерації).
    /// </summary>
    public string? ReviewText { get; set; }
}
