import { apiClient } from './api';
import type { BookScreenItem } from './types';
import { ALL_ITEMS } from './items';

export interface FetchWorksParams {
  genre?: string | null;
  country?: string | null;
  search?: string | null;
}

// Прапорець — чи дозволено fallback на локальні дані, коли бекенд недоступний.
// На захисті Етапу 3 вмикаємо; після того як бекенд стабільно працює — вимикаємо.
const USE_MOCK_FALLBACK = true;

/**
 * GET /api/v1/works
 * Тягне твори з бекенду. Якщо бекенд недоступний і USE_MOCK_FALLBACK=true,
 * повертає локальний список із items.ts (щоб демо не зламалось).
 */
export async function fetchWorks(params: FetchWorksParams = {}): Promise<BookScreenItem[]> {
  try {
    const response = await apiClient.get<BookScreenItem[]>('/api/v1/works', {
      params: {
        genre: params.genre || undefined,
        country: params.country || undefined,
        search: params.search || undefined,
      },
    });
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[works] Backend недоступний, використовуються локальні дані', err);
      return ALL_ITEMS;
    }
    throw err;
  }
}

/**
 * GET /api/v1/works/:id
 */
export async function fetchWorkById(id: number): Promise<BookScreenItem> {
  try {
    const response = await apiClient.get<BookScreenItem>(`/api/v1/works/${id}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      const local = ALL_ITEMS.find((i) => i.id === id);
      if (local) {
        console.warn('[works] Backend недоступний, повертаємо локальні дані для id=', id);
        return local;
      }
    }
    throw err;
  }
}
