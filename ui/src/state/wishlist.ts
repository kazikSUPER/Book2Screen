import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';
import { addToFavorites, removeFromFavorites, fetchFavorites } from '../services/favorites';
import { useUserStore } from './user';

/**
 * SCRUM-68 — позначки користувача "Хочу прочитати" і "Хочу переглянути"
 * на сторінці деталей твору.
 *
 * Зберігаються у localStorage окремими ключами per-workId, тому переживають
 * перезавантаження сторінки (вимога Етапу 4: persistent state).
 *
 * Синхронізуються з бекендом (BUG-023) через FavoritesController.
 */

export type WishKind = 'read' | 'watch';

interface WishlistEntry {
  workId: string;
  kind: WishKind;
}

export const useWishlistStore = defineStore('wishlist', () => {
  const userStore = useUserStore();
  const items = usePersistedRef<WishlistEntry[]>('b2s_wishlist', []);

  function isInWishlist(workId: string, kind: WishKind): boolean {
    return items.value.some((e) => e.workId === workId && e.kind === kind);
  }

  async function toggle(workId: string, kind: WishKind): Promise<void> {
    const idx = items.value.findIndex((e) => e.workId === workId && e.kind === kind);
    if (idx >= 0) {
      items.value.splice(idx, 1);
      if (userStore.isAuthenticated) {
        try {
          await removeFromFavorites(workId, kind);
        } catch (e) {
          console.error('[wishlist] Failed to remove from backend', e);
        }
      }
    } else {
      items.value.push({ workId, kind });
      if (userStore.isAuthenticated) {
        try {
          await addToFavorites(workId, kind);
        } catch (e) {
          console.error('[wishlist] Failed to add to backend', e);
        }
      }
    }
  }

  /**
   * Синхронізація з бекендом при логіні або відкритті профілю.
   */
  async function syncWithBackend(): Promise<void> {
    if (!userStore.isAuthenticated) return;
    try {
      const read = await fetchFavorites('read');
      const watch = await fetchFavorites('watch');

      const synced: WishlistEntry[] = [
        ...read.map(b => ({ workId: b.id, kind: 'read' as WishKind })),
        ...watch.map(b => ({ workId: b.id, kind: 'watch' as WishKind }))
      ];

      // Merge local with backend or just overwrite with backend?
      // Для MVP краще overwrite backend data як source of truth.
      items.value = synced;
    } catch (e) {
      console.error('[wishlist] Sync failed', e);
    }
  }

  // Списки id для ProfileView (SCRUM-64) — секція "Хочу переглянути/прочитати".
  const readList = computed(() => items.value.filter((e) => e.kind === 'read').map((e) => e.workId));
  const watchList = computed(() =>
    items.value.filter((e) => e.kind === 'watch').map((e) => e.workId)
  );

  return { items, isInWishlist, toggle, readList, watchList, syncWithBackend };
});
