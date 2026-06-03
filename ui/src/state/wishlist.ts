import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';
import * as favoritesApi from '../services/favorites';

/**
 * SCRUM-68 — позначки користувача "Хочу прочитати / Хочу переглянути".
 *
 * Архітектура (станом на підключення бекенду):
 *  • БЕК має /Favorites — повна синхронізація творів, які користувач додав.
 *  • Бек НЕ розрізняє 'read' / 'watch' — у нього просто список workId.
 *
 * Тому ми:
 *  1. Тримаємо локальний `kindMap`: { workId → 'read'|'watch'|'both' } у localStorage —
 *     для UI-розрізнення (на сторінці деталі дві окремі кнопки).
 *  2. Список workId синхронізуємо з беком (push/pull через /Favorites).
 *  3. При login → sync(), при logout → не чіпаємо локальне (анонімний режим).
 *
 * Якщо в майбутньому бек додасть поле `kind` у FavoriteRequest — позбавимось локального map.
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

    // Sync з беком. Бек не розрізняє kind — додаємо в /Favorites, якщо це перше
    // додавання цього workId; видаляємо тільки коли БУДЬ-якого kind не лишилось.
    const stillHasAnyKind = items.value.some((e) => e.workId === workId);
    try {
      if (!isRemoving && stillHasAnyKind) {
        await favoritesApi.addFavorite(workId);
      } else if (isRemoving && !stillHasAnyKind) {
        await favoritesApi.removeFavorite(workId);
      }
    } catch (err) {
      console.warn('[wishlist] Backend sync failed (ignored):', err);
    }
  }

  /** Початкова синхронізація з бекенду (викликати після login). */
  async function syncFromServer(): Promise<void> {
    try {
      const remote = await favoritesApi.fetchFavorites();
      const remoteIds = new Set(remote.map((w) => w.id));
      // Додаємо в локальний список ті, що є на сервері (kind='read' за замовчуванням).
      for (const id of remoteIds) {
        if (!items.value.some((e) => e.workId === id)) {
          items.value.push({ workId: id, kind: 'read' });
        }
      }
    } catch (err) {
      console.warn('[wishlist] Sync from server failed (ignored):', err);
    }
  }

  const readList = computed(() => items.value.filter((e) => e.kind === 'read').map((e) => e.workId));
  const watchList = computed(() => items.value.filter((e) => e.kind === 'watch').map((e) => e.workId));

  return { items, isInWishlist, toggle, syncFromServer, readList, watchList };
});
