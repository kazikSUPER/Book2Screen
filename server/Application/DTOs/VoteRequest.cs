// <copyright file="VoteRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Запит на голосування.
/// </summary>
public class VoteRequest
{
    /// <summary>
    /// Gets or sets iD твору (Work), за який проводиться голосування.
    /// </summary>
    [Required]
    public required Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets тип голосу: "book" (книга краща) або "movie" (адаптація краща).
    /// </summary>
    [Required]
    [RegularExpression("^(book|movie|BOOK|MOVIE)$", ErrorMessage = "VoteType must be 'book' or 'movie'")]
    public string VoteType { get; set; } = null!;
}
