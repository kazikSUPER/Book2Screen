// <copyright file="ReviewService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для роботи з відгуками користувачів.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewService"/> class.
    /// Ініціалізує новий екземпляр <see cref="ReviewService"/>.
    /// </summary>
    /// <param name="context">Контекст бази даних.</param>
    public ReviewService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<ReviewResponse> AddReviewAsync(Guid userId, ReviewRequest request)
    {
        var review = new Review
        {
            UserId = userId,
            WorkId = request.WorkId,
            Text = request.Text,
            IsSpoiler = request.IsSpoiler,
            Rating = request.Rating,
            TargetType = "comparison",
        };

        await this.context.Reviews.AddAsync(review);
        await this.context.SaveChangesAsync();

        return new ReviewResponse
        {
            ReviewId = review.Id,
            WorkId = review.WorkId,
            UserId = review.UserId ?? Guid.Empty,
            Text = review.Text,
            IsSpoiler = review.IsSpoiler,
            Rating = review.Rating,
            CreatedAt = review.CreatedAt,
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReviewResponse>> GetReviewsByWorkIdAsync(Guid workId)
    {
        return await this.context.Reviews
            .Where(r => r.WorkId == workId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.Id,
                WorkId = r.WorkId,
                UserId = r.UserId ?? Guid.Empty,
                Text = r.Text,
                IsSpoiler = r.IsSpoiler,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();
    }
}
