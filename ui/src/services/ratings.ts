import { apiClient } from './api';
import { USE_MOCK_FALLBACK } from './env';

export interface RatingRequest {
  workId: string;
  target: 'book' | 'film';
  value: number;
}

export async function submitRating(req: RatingRequest): Promise<void> {
  try {
    await apiClient.post('/api/v1/ratings', req);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[ratings] Backend unavailable, mock rating saved', req);
      return;
    }
    throw err;
  }
}
