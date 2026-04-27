// <copyright file="Rating.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Domain.Entities;

/// <summary>
/// Рейтинги та статистика голосування.
/// </summary>
public class Rating : BaseEntity
{
    /// <summary>
    /// Gets or sets iD твору.
    /// </summary>
    public Guid WorkId { get; set; }

    /// <summary>
    /// Gets or sets об'єкт твору.
    /// </summary>
    public Work Work { get; set; } = null!;

    /// <summary>
    /// Gets or sets середній рейтинг книги.
    /// </summary>
    public decimal? BookRating { get; set; }

    /// <summary>
    /// Gets or sets середній рейтинг адаптації.
    /// </summary>
    public decimal? AdaptationRating { get; set; }

    /// <summary>
    /// Gets or sets кількість голосів.
    /// </summary>
    public int VotesCount { get; set; } = 0;
}
