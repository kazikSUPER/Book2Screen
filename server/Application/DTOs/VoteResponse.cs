// <copyright file="VoteResponse.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Відповідь з результатами голосування.
/// </summary>
public class VoteResponse
{
    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets загальна кількість голосів.
    /// </summary>
    public int TotalVotes { get; set; }

    /// <summary>
    /// Gets or sets кількість голосів за книгу.
    /// </summary>
    public int BookVotes { get; set; }

    /// <summary>
    /// Gets or sets кількість голосів за адаптацію.
    /// </summary>
    public int MovieVotes { get; set; }

    /// <summary>
    /// Gets or sets відсоток голосів за книгу.
    /// </summary>
    public double BookPercentage { get; set; }

    /// <summary>
    /// Gets or sets відсоток голосів за адаптацію.
    /// </summary>
    public double MoviePercentage { get; set; }
}
