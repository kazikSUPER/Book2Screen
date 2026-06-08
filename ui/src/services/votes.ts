import { apiClient } from './api';
import type { VoteRequest, VoteResponse, VoteType } from './types';
import { USE_MOCK_FALLBACK } from './env';

/**
 * Votes API (Book2Screen v1).
 *
 * Endpoints (Swagger):
 *   GET  /api/v1/Votes/{workId} — поточна статистика голосів
 *   POST /api/v1/Votes          — проголосувати; повертає оновлену статистику.
 *
 * УВАГА: voteType — рядки lowercase 'book' | 'movie' (не 'BOOK'/'MOVIE').
 */

// POST /api/v1/Votes
export async function submitVote(workId: string, type: VoteType): Promise<VoteResponse> {
  const body: VoteRequest = { workId, voteType: type };
  try {
    const response = await apiClient.post<VoteResponse>('/api/v1/Votes', body);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[votes] Backend unavailable, returning mock', err);
      return {
        workId,
        totalVotes: 100,
        bookVotes: type === 'book' ? 51 : 50,
        movieVotes: type === 'movie' ? 50 : 50,
        bookPercentage: type === 'book' ? 51 : 50,
        moviePercentage: type === 'movie' ? 49 : 50,
      };
    }
    throw err;
  }
}

// GET /api/v1/Votes/{workId}
export async function fetchVoteResults(workId: string): Promise<VoteResponse> {
  try {
    const response = await apiClient.get<VoteResponse>(`/api/v1/Votes/${workId}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[votes] Backend unavailable, returning mock', err);
      return {
        workId,
        totalVotes: 100,
        bookVotes: 50,
        movieVotes: 50,
        bookPercentage: 50,
        moviePercentage: 50,
      };
    }
    throw err;
  }
}
