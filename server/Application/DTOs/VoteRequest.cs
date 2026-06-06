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
    /// Gets or sets тип голосу: "book", "movie" або "adaptation".
    /// </summary>
    [Required]
    [RegularExpression("^(book|movie|adaptation|BOOK|MOVIE|ADAPTATION)$", ErrorMessage = "VoteType must be 'book', 'movie' or 'adaptation'")]
    public string VoteType { get; set; } = null!;
}
