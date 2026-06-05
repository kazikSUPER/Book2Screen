import { apiClient } from './api';
import type { ReviewRequest, ReviewResponse } from './types';
import { USE_MOCK_FALLBACK } from './env';

/**
 * Reviews API (Book2Screen v1).
 *
 * Endpoints (Swagger):
 *   GET    /api/v1/Reviews/work/{workId}   — список відгуків до твору
 *   POST   /api/v1/Reviews                 — створити відгук
 *   PUT    /api/v1/Reviews/{id}            — оновити свій відгук
 *   DELETE /api/v1/Reviews/{id}            — видалити свій відгук
 *   POST   /api/v1/Reviews/{id}/report     — поскаржитись (body: рядок з причиною)
 *
 * ReviewRequest вимагає targetType: 'book' | 'adaptation' | 'comparison'.
 * За замовчуванням у UI використовуємо 'comparison' (відгук про твір цілком).
 */

const mockReviewsByWork: Map<string, ReviewResponse[]> = new Map();
if (USE_MOCK_FALLBACK) seedMockReviews();

// GET /api/v1/Reviews/work/{workId}
export async function fetchReviews(workId: string): Promise<ReviewResponse[]> {
  try {
    const response = await apiClient.get<ReviewResponse[]>(`/api/v1/Reviews/work/${workId}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[reviews] Backend unavailable, returning mock', err);
      return [...(mockReviewsByWork.get(workId) ?? [])];
    }
    throw err;
  }
}

// POST /api/v1/Reviews
export async function submitReview(req: ReviewRequest): Promise<ReviewResponse> {
  try {
    const response = await apiClient.post<ReviewResponse>('/api/v1/Reviews', req);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[reviews] Backend unavailable, saving locally', err);
      const fake: ReviewResponse = {
        reviewId: `local-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
        workId: req.workId,
        userId: 'me',
        userNickname: 'Ви',
        text: req.text,
        isSpoiler: req.isSpoiler,
        rating: req.rating,
        targetType: req.targetType,
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

// PUT /api/v1/Reviews/{id}
export async function updateReview(reviewId: string, req: ReviewRequest): Promise<void> {
  try {
    await apiClient.put(`/api/v1/Reviews/${reviewId}`, req);
  } catch (err) {
    if (!USE_MOCK_FALLBACK) throw err;
    console.warn('[reviews] Mock update:', reviewId);
  }
}

// DELETE /api/v1/Reviews/{id}
export async function deleteReview(reviewId: string): Promise<void> {
  try {
    await apiClient.delete(`/api/v1/Reviews/${reviewId}`);
  } catch (err) {
    if (!USE_MOCK_FALLBACK) throw err;
    console.warn('[reviews] Mock delete:', reviewId);
  }
}

// POST /api/v1/Reviews/{id}/report — тіло: { reason: string }
export async function reportReview(reviewId: string, reason: string): Promise<void> {
  try {
    // Бек чекає об'єкт ReportRequest: { reason: "..." }
    await apiClient.post(`/api/v1/Reviews/${reviewId}/report`, { reason });
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[reviews] Mock report:', { reviewId, reason });
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
      rating: 9,
      targetType: 'comparison',
      createdAt: '2024-12-10T14:32:00.000Z',
    },
    {
      reviewId: 'seed-hp-2',
      workId: hpId,
      userId: 'u-002',
      userNickname: 'Андрій',
      text: 'У книзі Гаррі більше думає, у фільмі більше дії. Обоє хороші, але…',
      isSpoiler: true,
      rating: 8,
      targetType: 'comparison',
      createdAt: '2024-12-08T19:11:00.000Z',
    },
    {
      reviewId: 'seed-hp-3',
      workId: hpId,
      userId: 'u-003',
      userNickname: 'Ольга',
      text: 'Перечитала до перегляду — книга все одно краща.',
      isSpoiler: false,
      rating: 10,
      targetType: 'book',
      createdAt: '2024-12-01T08:00:00.000Z',
    },
  ];
  mockReviewsByWork.set(hpId, seed);
}
