namespace Book2Screen.Application.Services;

using Book2Screen.Application.DTOs;
using Book2Screen.Application.Interfaces;
using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Сервіс для керування голосуваннями "Що краще: книга чи фільм?".
/// </summary>
public class VoteService : IVoteService
{
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="VoteService"/>.
    /// </summary>
    public VoteService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<VoteResponse> VoteAsync(Guid userId, VoteRequest request)
    {
        // Тимчасово відключено до застосування міграцій
        return await this.GetVoteStatsAsync(request.WorkId);
    }

    public async Task<VoteResponse> GetVoteStatsAsync(Guid workId)
    {
        // Повертаємо пусту статистику, поки таблиці не існує
        return new VoteResponse
        {
            WorkId = workId,
            TotalVotes = 0,
            BookVotes = 0,
            MovieVotes = 0,
            BookPercentage = 0,
            MoviePercentage = 0
        };
    }
}
