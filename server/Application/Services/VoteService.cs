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
        var existingVote = await this.context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.WorkId == request.WorkId);

        if (existingVote != null)
        {
            existingVote.SelectedOption = request.VoteType.ToLower();
        }
        else
        {
            var vote = new Vote
            {
                UserId = userId,
                WorkId = request.WorkId,
                SelectedOption = request.VoteType.ToLower()
            };
            await this.context.Votes.AddAsync(vote);
        }

        await this.context.SaveChangesAsync();
        return await this.GetVoteStatsAsync(request.WorkId);
    }

    public async Task<VoteResponse> GetVoteStatsAsync(Guid workId)
    {
        var votes = await this.context.Votes.Where(v => v.WorkId == workId).ToListAsync();
        
        int total = votes.Count;
        int bookVotes = votes.Count(v => v.SelectedOption == "book");
        int movieVotes = votes.Count(v => v.SelectedOption == "movie" || v.SelectedOption == "adaptation");

        return new VoteResponse
        {
            WorkId = workId,
            TotalVotes = total,
            BookVotes = bookVotes,
            MovieVotes = movieVotes,
            BookPercentage = total > 0 ? (double)bookVotes / total * 100 : 0,
            MoviePercentage = total > 0 ? (double)movieVotes / total * 100 : 0
        };
    }
}
