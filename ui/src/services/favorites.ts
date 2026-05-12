import { apiClient } from './api';
import type { BookScreenItem } from './types';

/**
 * Сервіс для роботи з обраним (Favorites).
 * Backend: FavoritesController (api/v1/favorites)
 */

export async function fetchFavorites(): Promise<BookScreenItem[]> {
  const response = await apiClient.get<BookScreenItem[]>('/api/v1/favorites');
  return response.data;
}

export async function addToFavorites(workId: string): Promise<void> {
  await apiClient.post('/api/v1/favorites', { workId });
}

export async function removeFromFavorites(workId: string): Promise<void> {
  await apiClient.delete(`/api/v1/favorites/${workId}`);
}

export async function checkIsFavorite(workId: string): Promise<boolean> {
  const response = await apiClient.get<boolean>(`/api/v1/favorites/check/${workId}`);
  return response.data;
}
