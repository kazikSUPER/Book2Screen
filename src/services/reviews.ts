import { apiClient } from './api';
import type { ReviewRequest, ReviewResponse } from './types';
import { USE_MOCK_FALLBACK } from './env';

/**
 * SCRUM-72 (US 6.1) — Writing Review.
 *
 * Backend: POST /api/v1/reviews         — створити відгук
 *          GET  /api/v1/reviews/:workId — список відгуків для твору
 *          POST /api/v1/reviews/:id/report — поскаржитись (адмінська модерація)
 *
 * Поки бекенд не готовий — mock-fallback тримає список у пам'яті
 * (закумулюється на час сесії, скидається при перезавантаженні).
 */

// In-memory лог mock-відгуків. Ключ — workId, значення — масив відгуків.
const mockReviewsByWork: Map<string, ReviewResponse[]> = new Map();

// Стартові mock-відгуки для демонстрації UI (тільки коли USE_MOCK_FALLBACK).
if (USE_MOCK_FALLBACK) seedMockReviews();

export async function fetchReviews(workId: string): Promise<ReviewResponse[]> {
  try {
    const response = await apiClient.get<ReviewResponse[]>(`/api/v1/reviews/${workId}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[reviews] Backend unavailable, returning mock list', err);
      return [...(mockReviewsByWork.get(workId) ?? [])];
    }
    throw err;
  }
}

export async function submitReview(req: ReviewRequest): Promise<ReviewResponse> {
  try {
    const response = await apiClient.post<ReviewResponse>('/api/v1/reviews', req);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[reviews] Backend unavailable, saving review locally', err);
      const fake: ReviewResponse = {
        reviewId: `local-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
        workId: req.workId,
        userId: 'me',
        userNickname: 'Ви',
        text: req.text,
        isSpoiler: req.isSpoiler,
        rating: req.rating,
        createdAt: new Date().toISOString(),
      };
      const list = mockReviewsByWork.get(req.workId) ?? [];
      list.unshift(fake);
      mockReviewsByWork.set(req.workId, list);
      return fake;
    }
    throw err;
  }
}

export async function reportReview(reviewId: string, reason: string): Promise<void> {
  try {
    await apiClient.post(`/api/v1/reviews/${reviewId}/report`, { reason });
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[reviews] Report sent locally (mock):', { reviewId, reason });
      return;
    }
    throw err;
  }
}

function seedMockReviews(): void {
  const hpId = '11111111-1111-1111-1111-111111111111';
  const seed: ReviewResponse[] = [
    {
      reviewId: 'seed-hp-1',
      workId: hpId,
      userId: 'u-001',
      userNickname: 'Світлана',
      text: 'Книга глибша, але фільм — чарівний для дітей! Дуже сподобалось.',
      isSpoiler: false,
      rating: 5,
      createdAt: '2024-12-10T14:32:00.000Z',
    },
    {
      reviewId: 'seed-hp-2',
      workId: hpId,
      userId: 'u-002',
      userNickname: 'Андрій',
      text: 'У книзі Гаррі більше думає, у фільмі більше дії. Обоє хороші, але...',
      isSpoiler: true,
      rating: 4,
      createdAt: '2024-12-08T19:11:00.000Z',
    },
    {
      reviewId: 'seed-hp-3',
      workId: hpId,
      userId: 'u-003',
      userNickname: 'Ольга',
      text: 'Перечитала до перегляду — книга все одно краща.',
      isSpoiler: false,
      rating: 5,
      createdAt: '2024-12-01T08:00:00.000Z',
    },
  ];
  mockReviewsByWork.set(hpId, seed);
}
