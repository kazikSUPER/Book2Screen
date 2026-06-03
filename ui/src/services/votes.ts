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
      return mockResult(workId, type);
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
      return mockResult(workId, null);
    }
    throw err;
  }
}

function mockResult(workId: string, addType: VoteType | null): VoteResponse {
  const seed = simpleHash(workId);
  const baseTotal = 80 + (seed % 200);
  const bookBias = (seed % 60) - 30;
  let bookVotes = Math.max(1, Math.floor(baseTotal / 2 + bookBias / 2));
  let movieVotes = Math.max(1, baseTotal - bookVotes);
  if (addType === 'book') bookVotes += 1;
  else if (addType === 'movie') movieVotes += 1;
  const totalVotes = bookVotes + movieVotes;
  const bookPercentage = Math.round((bookVotes / totalVotes) * 100);
  const moviePercentage = 100 - bookPercentage;
  return { workId, totalVotes, bookVotes, movieVotes, bookPercentage, moviePercentage };
}

function simpleHash(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) {
    h = (h << 5) - h + s.charCodeAt(i);
    h |= 0;
  }
  return Math.abs(h);
}
