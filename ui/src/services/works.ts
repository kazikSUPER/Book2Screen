import { apiClient } from './api';
import type { BookScreenItem } from './types';
import { ALL_ITEMS } from './items';
import { USE_MOCK_FALLBACK } from './env';
import { mapGenreToBackend, mapCountryToBackend } from '../constants/catalog';

export interface FetchWorksParams {
  search?: string | null;
  genre?: string | null;
  country?: string | null;
  onlyWithMap?: boolean | null;
}

export async function fetchWorks(params: FetchWorksParams = {}): Promise<BookScreenItem[]> {
  try {
    const response = await apiClient.get<BookScreenItem[]>('/api/v1/Works', {
      params: {
        Search: params.search || undefined,
        Genre: mapGenreToBackend(params.genre) || undefined,
        Country: mapCountryToBackend(params.country) || undefined,
        OnlyWithMap: params.onlyWithMap || undefined,
      },
    });
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[works] Backend unavailable, using mock', err);
      return ALL_ITEMS;
    }
    throw err;
  }
}

export async function fetchWorkById(id: string): Promise<BookScreenItem> {
  try {
    const response = await apiClient.get<BookScreenItem>(`/api/v1/Works/${id}`);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      const local = ALL_ITEMS.find((i) => i.id === id);
      if (local) {
        console.warn('[works] Backend unavailable, returning mock for id=', id);
        return local;
      }
    }
    throw err;
  }
}

export async function fetchTopWorks(count = 10): Promise<BookScreenItem[]> {
  try {
    const response = await apiClient.get<BookScreenItem[]>('/api/v1/Works/top', {
      params: { count },
    });
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[works] Backend unavailable, returning mock top', err);
      return ALL_ITEMS.slice(0, count);
    }
    throw err;
  }
}
