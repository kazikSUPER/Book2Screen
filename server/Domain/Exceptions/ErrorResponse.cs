// <copyright file="ErrorResponse.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Exceptions;

/// <summary>
/// Стандартна модель відповіді при виникненні помилки.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets hTTP статус-код (напр. 400, 404, 500).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets машинно-зчитуваний код помилки (напр. "INTERNAL_ERROR", "NOT_FOUND").
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets короткий опис помилки для користувача.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets час виникнення помилки (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }
}
