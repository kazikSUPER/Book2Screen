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
    [Required]
    public string Reason { get; set; } = null!;
}
