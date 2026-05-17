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
        var workExists = await this.context.Works.AnyAsync(w => w.Id == request.WorkId);
        if (!workExists)
        {
            throw new KeyNotFoundException($"Work with ID {request.WorkId} not found.");
        }

        var review = new Review
        {
            UserId = userId,
            WorkId = request.WorkId,
            Text = request.Text,
            IsSpoiler = request.IsSpoiler,
            Rating = request.Rating,
            TargetType = request.TargetType.ToLower(),
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
            TargetType = review.TargetType,
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
                TargetType = r.TargetType,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateReviewAsync(Guid userId, Guid reviewId, ReviewRequest request)
    {
        var review = await this.context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null || review.UserId != userId)
        {
            return false;
        }

        review.Text = request.Text;
        review.IsSpoiler = request.IsSpoiler;
        review.Rating = request.Rating;
        review.TargetType = request.TargetType.ToLower();

        await this.context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
    {
        var review = await this.context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null || review.UserId != userId)
        {
            return false;
        }

        this.context.Reviews.Remove(review);
        await this.context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReviewResponse>> GetUserReviewsAsync(Guid userId)
    {
        return await this.context.Reviews
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.Id,
                WorkId = r.WorkId,
                UserId = r.UserId ?? Guid.Empty,
                Text = r.Text,
                IsSpoiler = r.IsSpoiler,
                Rating = r.Rating,
                TargetType = r.TargetType,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();
    }
}
