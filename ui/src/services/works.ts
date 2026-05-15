import { apiClient } from './api';
import type { BookScreenItem } from './types';
import { ALL_ITEMS } from './items';
import type { SortOption } from '../state/filters';

export interface FetchWorksParams {
  genre?: string | null;
  country?: string | null;
  search?: string | null;
  yearMin?: number | null;
  yearMax?: number | null;
  minRating?: number | null;
  sortBy?: SortOption | null;
  // SCRUM-67: чекбокс "Лише з картою відмінностей" у TopView.
  onlyWithMap?: boolean | null;
}

const USE_MOCK_FALLBACK = true;

/**
 * GET /api/v1/works
 * Тягне твори з бекенду. Якщо бекенд недоступний і USE_MOCK_FALLBACK=true,
 * повертає локальний список із items.ts (щоб демо не зламалось).
 *
 * Усі фільтри передаються в query-string. Backend має ігнорувати undefined.
 */
export async function fetchWorks(params: FetchWorksParams = {}): Promise<BookScreenItem[]> {
  try {
    const response = await apiClient.get<BookScreenItem[]>('/api/v1/works', {
      params: {
        genre: params.genre || undefined,
        country: params.country || undefined,
        search: params.search || undefined,
        yearMin: params.yearMin ?? undefined,
        yearMax: params.yearMax ?? undefined,
        minRating: params.minRating ?? undefined,
        sortBy: params.sortBy || undefined,
        onlyWithMap: params.onlyWithMap || undefined,
      },
    });
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[works] Backend unavailable, using local mock data', err);
      return ALL_ITEMS;
    }
    throw err;
  }
}

/**
 * GET /api/v1/works/:id
 * id is a GUID string (postgres uuid), not a number.
 */
export async function fetchWorkById(id: string): Promise<BookScreenItem> {
  try {
    const response = await apiClient.get<BookScreenItem>(`/api/v1/works/${id}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      const local = ALL_ITEMS.find((i) => i.id === id);
      if (local) {
        console.warn('[works] Backend unavailable, returning local mock for id=', id);
        return local;
      }
    }
    throw err;
  }
}
