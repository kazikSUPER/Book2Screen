import { apiClient } from './api';
import type { BookScreenItem, FavoriteRequest } from './types';
import { USE_MOCK_FALLBACK } from './env';

/**
 * Favorites API (Book2Screen v1).
 *
 * Endpoints (Swagger):
 *   GET    /api/v1/Favorites               — список улюблених творів
 *   POST   /api/v1/Favorites               — додати в улюблені (body: { workId })
 *   DELETE /api/v1/Favorites/{workId}      — видалити з улюблених
 *   GET    /api/v1/Favorites/check/{workId} — чи є твір в улюблених (повертає bool)
 *
 * На фронті це замінює локальний wishlist у localStorage — синхронізується з беком.
 */

// GET /api/v1/Favorites
export async function fetchFavorites(kind?: string): Promise<BookScreenItem[]> {
  try {
    const response = await apiClient.get<BookScreenItem[]>('/api/v1/Favorites', {
      params: kind ? { kind } : {},
    });
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[favorites] Backend unavailable, returning []', err);
      return [];
    }
    throw err;
  }
}

// POST /api/v1/Favorites
export async function addFavorite(workId: string, kind: string): Promise<void> {
  const body: FavoriteRequest = { workId, kind };
  try {
    await apiClient.post('/api/v1/Favorites', body);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[favorites] Mock add:', workId, kind);
      return;
    }
    throw err;
  }
}

// DELETE /api/v1/Favorites/{workId}
export async function removeFavorite(workId: string, kind?: string): Promise<void> {
  try {
    await apiClient.delete(`/api/v1/Favorites/${workId}`, {
      params: kind ? { kind } : {},
    });
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[favorites] Mock remove:', workId, kind);
      return;
    }
    throw err;
  }
}

// GET /api/v1/Favorites/check/{workId} → bool
export async function checkFavorite(workId: string, kind?: string): Promise<boolean> {
  try {
    const response = await apiClient.get<boolean>(`/api/v1/Favorites/check/${workId}`, {
      params: kind ? { kind } : {},
    });
    return response.data === true;
  } catch (err) {
    if (USE_MOCK_FALLBACK) return false;
    throw err;
  }
}
