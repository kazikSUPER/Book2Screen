// <copyright file="ReportRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит на створення скарги.
/// </summary>
public class ReportRequest
{
    /// <summary>
    /// Gets or sets причину скарги.
    /// </summary>
    [Required(ErrorMessage = "Reason is required")]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Gets or sets iD відгуку (опціонально, якщо передається в тілі).
    /// </summary>
    public Guid? ReviewId { get; set; }

    /// <summary>
    /// Gets or sets текст скарги (аліас для Reason).
    /// </summary>
    public string? Text { get; set; }
}
