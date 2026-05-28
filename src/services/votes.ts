import { apiClient } from './api';
import type { VoteRequest, VoteResponse, VoteType } from './types';
import { USE_MOCK_FALLBACK } from './env';

/**
 * SCRUM-70 / SCRUM-71 — голосування "Книга vs Фільм".
 *
 * Backend: POST /api/v1/votes — створює голос; повертає актуальний
 * розподіл (VoteResponse).
 * GET /api/v1/votes/:workId — забрати поточний результат.
 *
 * Поки бекенд не готовий — повертаємо мок-результат, який гарантовано
 * не зламає UI (зберігає відсотки книги/фільму довкола 50/50).
 */

export async function submitVote(workId: string, type: VoteType): Promise<VoteResponse> {
  const body: VoteRequest = { workId, voteType: type };
  try {
    const response = await apiClient.post<VoteResponse>('/api/v1/votes', body);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[votes] Backend unavailable, returning local mock', err);
      return mockResult(workId, type);
    }
    throw err;
  }
}

export async function fetchVoteResults(workId: string): Promise<VoteResponse> {
  try {
    const response = await apiClient.get<VoteResponse>(`/api/v1/votes/${workId}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[votes] Backend unavailable, returning mock results', err);
      return mockResult(workId, null);
    }
    throw err;
  }
}

// Детермінований mock — спирається на хеш workId, щоб різні твори мали різні
// "стабільні" відсотки. Якщо передано тип — додаємо +1 до відповідного боку.
function mockResult(workId: string, addType: VoteType | null): VoteResponse {
  const seed = simpleHash(workId);
  const baseTotal = 80 + (seed % 200); // 80..279
  const bookBias = (seed % 60) - 30; // -30..+29
  let bookVotes = Math.max(1, Math.floor(baseTotal / 2 + bookBias / 2));
  let movieVotes = Math.max(1, baseTotal - bookVotes);

  if (addType === 'BOOK') bookVotes += 1;
  else if (addType === 'MOVIE') movieVotes += 1;

  const totalVotes = bookVotes + movieVotes;
  const bookPercentage = Math.round((bookVotes / totalVotes) * 100);
  const moviePercentage = 100 - bookPercentage;

  return {
    workId,
    totalVotes,
    bookVotes,
    movieVotes,
    bookPercentage,
    moviePercentage,
  };
}

function simpleHash(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) {
    h = (h << 5) - h + s.charCodeAt(i);
    h |= 0;
  }
  return Math.abs(h);
}
