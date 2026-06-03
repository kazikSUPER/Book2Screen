import { apiClient } from './api';
import type { ReviewResponse, UserProfileDto } from './types';
import { USE_MOCK_FALLBACK } from './env';

/**
 * User Profile API (Book2Screen v1).
 *
 * Endpoints (Swagger):
 *   GET  /api/v1/users/me            — отримати профіль (UserProfileDto)
 *   PUT  /api/v1/users/me            — оновити профіль (body: UserProfileDto)
 *   POST /api/v1/users/me/avatar     — оновити URL аватара (body: рядок з URL)
 *   GET  /api/v1/users/me/reviews    — мої відгуки
 *
 * УВАГА: на беку немає поля `birthDate`. Зберігаємо `username`, `email`, `avatarUrl`, `joinedAt`.
 */

// GET /api/v1/users/me
export async function fetchMyProfile(): Promise<UserProfileDto> {
  try {
    const response = await apiClient.get<UserProfileDto>('/api/v1/users/me');
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Backend unavailable, returning mock profile', err);
      return {
        username: 'Ви',
        email: 'demo@book2screen.local',
        avatarUrl: '',
        joinedAt: new Date().toISOString(),
      };
    }
    throw err;
  }
}

// PUT /api/v1/users/me
export async function updateMyProfile(patch: UserProfileDto): Promise<void> {
  try {
    await apiClient.put('/api/v1/users/me', patch);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Mock update profile:', patch);
      return;
    }
    throw err;
  }
}

// POST /api/v1/users/me/avatar — body: рядок з URL
export async function updateMyAvatar(avatarUrl: string): Promise<void> {
  try {
    await apiClient.post('/api/v1/users/me/avatar', avatarUrl, {
      headers: { 'Content-Type': 'application/json' },
    });
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Mock avatar update:', avatarUrl.slice(0, 60));
      return;
    }
    throw err;
  }
}

// GET /api/v1/users/me/reviews
export async function fetchMyReviews(): Promise<ReviewResponse[]> {
  try {
    const response = await apiClient.get<ReviewResponse[]>('/api/v1/users/me/reviews');
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Backend unavailable, returning mock reviews', err);
      const now = new Date().toISOString();
      return [
        {
          reviewId: 'me-1',
          workId: '11111111-1111-1111-1111-111111111111',
          userId: 'me',
          userNickname: 'Ви',
          text: 'Чарівний світ — і книга, і фільм. Перечитую щороку.',
          isSpoiler: false,
          rating: 10,
          targetType: 'comparison',
          createdAt: now,
        },
      ];
    }
    throw err;
  }
}

// DELETE /api/v1/Reviews/{id} — використовуємо звідси для ProfileView.
// Бек не має /users/me/reviews/{id} — видалення через Reviews controller.
export async function deleteMyReview(reviewId: string): Promise<void> {
  try {
    await apiClient.delete(`/api/v1/Reviews/${reviewId}`);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[profile] Mock delete review:', reviewId);
      return;
    }
    throw err;
  }
}
