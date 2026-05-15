import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';
import * as favoritesApi from '../services/favorites';
import { useUserStore } from './user';

/**
 * Стор улюблених творів.
 *
 * Зберігає тільки ID (рядки-GUID).
 * Дані зберігаються у localStorage + синхронізуються з бекендом, якщо користувач авторизований.
 */
export const useFavoritesStore = defineStore('favorites', () => {
  const ids = usePersistedRef<string[]>('b2s_favorites', []);
  const userStore = useUserStore();

  const count = computed(() => ids.value.length);

  function isFavorite(workId: string): boolean {
    return ids.value.includes(workId);
  }

  async function fetchFromBackend(): Promise<void> {
    if (!userStore.isAuthenticated) return;
    try {
      const remote = await favoritesApi.fetchFavorites();
      ids.value = remote.map((f) => f.id);
    } catch (err) {
      console.error('[favorites] Failed to fetch from backend', err);
    }
  }

  async function add(workId: string): Promise<void> {
    if (!ids.value.includes(workId)) {
      ids.value = [...ids.value, workId];
      if (userStore.isAuthenticated) {
        try {
          await favoritesApi.addToFavorites(workId);
        } catch (err) {
          console.error('[favorites] Failed to add on backend', err);
        }
      }
    }
  }

  async function remove(workId: string): Promise<void> {
    ids.value = ids.value.filter((id) => id !== workId);
    if (userStore.isAuthenticated) {
      try {
        await favoritesApi.removeFromFavorites(workId);
      } catch (err) {
        console.error('[favorites] Failed to remove on backend', err);
      }
    }
  }

  async function toggle(workId: string): Promise<void> {
    if (isFavorite(workId)) {
      await remove(workId);
    } else {
      await add(workId);
    }
  }

  function clear(): void {
    ids.value = [];
  }

  return {
    ids,
    count,
    isFavorite,
    fetchFromBackend,
    add,
    remove,
    toggle,
    clear,
  };
});
