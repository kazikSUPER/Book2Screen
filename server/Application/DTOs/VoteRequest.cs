// <copyright file="VoteRequest.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.DTOs;

/// <summary>
/// Запит на голосування.
/// </summary>
public class VoteRequest
{
    /// <summary>
    /// Gets or sets iD твору (Work), за який проводиться голосування.
    /// </summary>
    public required Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets тип голосу: "book" (книга краща) або "movie" (адаптація краща).
    /// </summary>
    public string VoteType { get; set; } = null!; // "BOOK" or "MOVIE"
}
