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
      return { workId, totalVotes: 1, bookVotes: type === 'book' ? 1 : 0, movieVotes: type === 'movie' ? 1 : 0, bookPercentage: type === 'book' ? 100 : 0, moviePercentage: type === 'movie' ? 100 : 0 };
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
      return { workId, totalVotes: 0, bookVotes: 0, movieVotes: 0, bookPercentage: 0, moviePercentage: 0 };
    }
    throw err;
  }
}
