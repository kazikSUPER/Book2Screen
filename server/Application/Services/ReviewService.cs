// <copyright file="ReviewService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Domain.Exceptions;
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

        var user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new UnauthorizedException("User not found.");
        }

        using var transaction = await this.context.Database.BeginTransactionAsync();
        try
        {
            var targetTypeLower = request.TargetType.ToLower();
            var existingReview = await this.context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.WorkId == request.WorkId && r.TargetType == targetTypeLower);

            Review review;
            if (existingReview != null)
            {
                review = existingReview;
                review.Text = request.Text ?? review.Text; // Keep old text if new is null
                review.IsSpoiler = request.IsSpoiler;
                review.Rating = request.Rating;
            }
            else
            {
                review = new Review
                {
                    UserId = userId,
                    WorkId = request.WorkId,
                    Text = request.Text,
                    IsSpoiler = request.IsSpoiler,
                    Rating = request.Rating,
                    TargetType = targetTypeLower,
                };
                await this.context.Reviews.AddAsync(review);
            }

            await this.context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ReviewResponse
            {
                ReviewId = review.Id,
                WorkId = review.WorkId,
                UserId = review.UserId ?? Guid.Empty,
                UserNickname = user.Username,
                UserAvatar = user.AvatarUrl,
                Text = review.Text,
                IsSpoiler = review.IsSpoiler,
                Rating = review.Rating,
                TargetType = review.TargetType,
                CreatedAt = review.CreatedAt,
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReviewResponse>> GetReviewsByWorkIdAsync(Guid workId)
    {
        return await this.context.Reviews
            .Include(r => r.User)
            .Where(r => r.WorkId == workId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.Id,
                WorkId = r.WorkId,
                UserId = r.UserId ?? Guid.Empty,
                UserNickname = r.User!.Username,
                UserAvatar = r.User!.AvatarUrl,
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
        if (review == null)
        {
            throw new KeyNotFoundException($"Review with ID {reviewId} not found.");
        }

        if (review.UserId != userId)
        {
            throw new ForbiddenException("You can only update your own reviews.");
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
        if (review == null)
        {
            throw new KeyNotFoundException($"Review with ID {reviewId} not found.");
        }

        if (review.UserId != userId)
        {
            throw new ForbiddenException("You can only delete your own reviews.");
        }

        this.context.Reviews.Remove(review);
        await this.context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReviewResponse>> GetUserReviewsAsync(Guid userId)
    {
        return await this.context.Reviews
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.Id,
                WorkId = r.WorkId,
                UserId = r.UserId ?? Guid.Empty,
                UserNickname = r.User!.Username,
                UserAvatar = r.User!.AvatarUrl,
                Text = r.Text,
                IsSpoiler = r.IsSpoiler,
                Rating = r.Rating,
                TargetType = r.TargetType,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task ReportReviewAsync(Guid userId, Guid reviewId, string reason)
    {
        var report = new Report
        {
            UserId = userId,
            ReviewId = reviewId,
            Reason = reason,
            Status = "Pending",
        };

        await this.context.Reports.AddAsync(report);
        await this.context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReportResponse>> GetAllReportsAsync()
    {
        return await this.context.Reports
            .Include(r => r.Review)
                .ThenInclude(rev => rev!.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportResponse
            {
                ReportId = r.Id,
                ReviewId = r.ReviewId ?? Guid.Empty,
                UserId = r.UserId ?? Guid.Empty,
                Reason = r.Reason,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ReviewText = r.Review != null ? r.Review.Text : "Review deleted",
                Review = r.Review == null ? null : new ReviewResponse
                {
                    ReviewId = r.Review.Id,
                    WorkId = r.Review.WorkId,
                    UserId = r.Review.UserId ?? Guid.Empty,
                    UserNickname = r.Review.User!.Username ?? "Deleted User",
                    UserAvatar = r.Review.User!.AvatarUrl,
                    Text = r.Review.Text,
                    IsSpoiler = r.Review.IsSpoiler,
                    Rating = r.Review.Rating,
                    TargetType = r.Review.TargetType,
                    CreatedAt = r.Review.CreatedAt,
                },
            })
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task ModerateReviewAsync(Guid reportId, string action)
    {
        var report = await this.context.Reports
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
        {
            throw new KeyNotFoundException($"Report with ID {reportId} not found.");
        }

        using var transaction = await this.context.Database.BeginTransactionAsync();
        try
        {
            switch (action.ToLower())
            {
                case "approve":
                    if (report.Review != null)
                    {
                        this.context.Reviews.Remove(report.Review);
                    }

                    report.Status = "Resolved";
                    break;
                case "reject":
                    report.Status = "Dismissed";
                    break;
                case "spoiler":
                    if (report.Review != null)
                    {
                        report.Review.IsSpoiler = true;
                    }

                    report.Status = "Resolved";
                    break;
                default:
                    throw new ArgumentException("Invalid action. Use approve, reject, or spoiler.");
            }

            await this.context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
