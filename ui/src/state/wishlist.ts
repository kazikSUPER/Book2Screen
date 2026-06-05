import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';
import * as favoritesApi from '../services/favorites';

/**
 * SCRUM-68 — позначки користувача "Хочу прочитати / Хочу переглянути".
 *
 * Архітектура (станом на підключення бекенду):
 *  • БЕК має /Favorites — повна синхронізація творів, які користувач додав.
 *  • Бек розрізняє 'read' / 'watch' через поле kind у FavoriteRequest та запитах.
 *
 * Тому ми:
 *  1. Тримаємо локальний список { workId, kind } у localStorage —
 *     для швидкого UI.
 *  2. Синхронізуємо з беком (push/pull через /Favorites).
 *  3. При login → sync(), при logout → не чіпаємо локальне (анонімний режим).
 */

export type WishKind = 'read' | 'watch';

interface WishlistEntry {
  workId: string;
  kind: WishKind;
}

export const useWishlistStore = defineStore('wishlist', () => {
  const items = usePersistedRef<WishlistEntry[]>('b2s_wishlist', []);

  function isInWishlist(workId: string, kind: WishKind): boolean {
    return items.value.some((e) => e.workId === workId && e.kind === kind);
  }

  /**
   * Локальний toggle + спроба синхронізації з беком.
   * Якщо бек неавторизований/недоступний — лишається тільки локально.
   */
  async function toggle(workId: string, kind: WishKind): Promise<void> {
    const idx = items.value.findIndex((e) => e.workId === workId && e.kind === kind);
    const isRemoving = idx >= 0;

    if (isRemoving) {
      items.value.splice(idx, 1);
    } else {
      items.value.push({ workId, kind });
    }

    try {
      if (!isRemoving) {
        await favoritesApi.addFavorite(workId, kind);
      } else {
        await favoritesApi.removeFavorite(workId, kind);
      }
    } catch (err) {
      console.warn('[wishlist] Backend sync failed (ignored):', err);
    }
  }

  /** Початкова синхронізація з бекенду (викликати після login). */
  async function syncFromServer(): Promise<void> {
    try {
      // Отримуємо окремо прочитані та переглянуті
      const [readListRemote, watchListRemote] = await Promise.all([
        favoritesApi.fetchFavorites('read'),
        favoritesApi.fetchFavorites('watch')
      ]);

      const newItems: WishlistEntry[] = [];

      readListRemote.forEach(w => newItems.push({ workId: w.id, kind: 'read' }));
      watchListRemote.forEach(w => newItems.push({ workId: w.id, kind: 'watch' }));

      // Замінюємо локальний список на серверний (оскільки сервер — джерело істини)
      items.value = newItems;
    } catch (err) {
      console.warn('[wishlist] Sync from server failed (ignored):', err);
    }
  }

  const readList = computed(() => items.value.filter((e) => e.kind === 'read').map((e) => e.workId));
  const watchList = computed(() => items.value.filter((e) => e.kind === 'watch').map((e) => e.workId));

  return { items, isInWishlist, toggle, syncFromServer, readList, watchList };
});
