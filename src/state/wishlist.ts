import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';

/**
 * SCRUM-68 — позначки користувача "Хочу прочитати" і "Хочу переглянути"
 * на сторінці деталей твору.
 *
 * Зберігаються у localStorage окремими ключами per-workId, тому переживають
 * перезавантаження сторінки (вимога Етапу 4: persistent state).
 *
 * Коли бекенд буде готовий — sync через POST /api/v1/wishlist (TODO).
 */

export type WishKind = 'read' | 'watch';

interface WishlistEntry {
  workId: string;
  kind: WishKind;
}

export const useWishlistStore = defineStore('wishlist', () => {
  // Зберігаємо як масив, щоб не плодити N localStorage ключів.
  const items = usePersistedRef<WishlistEntry[]>('b2s_wishlist', []);

  function isInWishlist(workId: string, kind: WishKind): boolean {
    return items.value.some((e) => e.workId === workId && e.kind === kind);
  }

  function toggle(workId: string, kind: WishKind): void {
    const idx = items.value.findIndex((e) => e.workId === workId && e.kind === kind);
    if (idx >= 0) {
      items.value.splice(idx, 1);
    } else {
      items.value.push({ workId, kind });
    }
  }

  // Списки id для ProfileView (SCRUM-64) — секція "Хочу переглянути/прочитати".
  const readList = computed(() => items.value.filter((e) => e.kind === 'read').map((e) => e.workId));
  const watchList = computed(() => items.value.filter((e) => e.kind === 'watch').map((e) => e.workId));

  return { items, isInWishlist, toggle, readList, watchList };
});
