import { apiClient } from './api';
import type { ReviewResponse } from './types';

/**
 * SCRUM-64 — Personal Profile.
 *
 * Backend endpoints (заплановані):
 *   GET  /api/v1/users/me/reviews       — мої відгуки
 *   PUT  /api/v1/users/me               — оновити профіль
 *   POST /api/v1/users/me/avatar        — завантажити аватар
 *
 * Поки бекенд не готовий — повертаємо мок-список відгуків з тих самих
 * сидових даних, які seed'ить services/reviews.ts.
 */

const USE_MOCK_FALLBACK = true;

export async function fetchMyReviews(): Promise<ReviewResponse[]> {
  try {
    const response = await apiClient.get<ReviewResponse[]>('/api/v1/users/me/reviews');
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Backend unavailable, returning mock reviews', err);
      // Кілька прикладів — щоб у ProfileView було що показати.
      const now = new Date().toISOString();
      return [
        {
          reviewId: 'me-1',
          workId: '11111111-1111-1111-1111-111111111111',
          userId: 'me',
          userNickname: 'Ви',
          text: 'Чарівний світ — і книга, і фільм. Перечитую щороку.',
          isSpoiler: false,
          rating: 5,
          createdAt: now,
        },
        {
          reviewId: 'me-2',
          workId: '22222222-2222-2222-2222-222222222222',
          userId: 'me',
          userNickname: 'Ви',
          text: 'Мабуть, найкраща екранізація трилогії з усіх можливих.',
          isSpoiler: false,
          rating: 5,
          createdAt: now,
        },
      ];
    }
    throw err;
  }
}

export async function deleteMyReview(reviewId: string): Promise<void> {
  try {
    await apiClient.delete(`/api/v1/reviews/${reviewId}`);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Mock delete review:', reviewId);
      return;
    }
    throw err;
  }
}

export async function updateMyReview(reviewId: string, text: string, isSpoiler: boolean): Promise<void> {
  try {
    await apiClient.put(`/api/v1/reviews/${reviewId}`, { text, isSpoiler });
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Mock update review:', { reviewId, text, isSpoiler });
      return;
    }
    throw err;
  }
}
