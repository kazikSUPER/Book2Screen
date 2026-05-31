import { apiClient } from './api';
import type { BookScreenItem } from './types';

/**
 * Сервіс для роботи з обраним (Favorites).
 * Backend: FavoritesController (api/v1/favorites)
 */

export async function fetchFavorites(kind?: string): Promise<BookScreenItem[]> {
  const url = kind ? `/api/v1/favorites?kind=${kind}` : '/api/v1/favorites';
  const response = await apiClient.get<BookScreenItem[]>(url);
  return response.data;
}

export async function addToFavorites(workId: string, kind: string = 'favorite'): Promise<void> {
  await apiClient.post('/api/v1/favorites', { workId, kind });
}

export async function removeFromFavorites(workId: string, kind?: string): Promise<void> {
  const url = kind ? `/api/v1/favorites/${workId}?kind=${kind}` : `/api/v1/favorites/${workId}`;
  await apiClient.delete(url);
}

export async function checkIsFavorite(workId: string, kind?: string): Promise<boolean> {
  const url = kind ? `/api/v1/favorites/check/${workId}?kind=${kind}` : `/api/v1/favorites/check/${workId}`;
  const response = await apiClient.get<boolean>(url);
  return response.data;
}
