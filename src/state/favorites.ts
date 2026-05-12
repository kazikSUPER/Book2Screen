import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';

/**
 * Стор улюблених творів.
 *
 * Зберігає тільки ID (рядки-GUID). Самі обʼєкти BookScreenItem
 * тягнемо з works API за потреби — щоб не дублювати дані між сторінками
 * і щоб коли бекенд оновить твір, ми бачили актуальну версію, а не закешовану.
 *
 * Дані зберігаються у localStorage — тому стан синхронізований
 * між Home, Search, Detail без жодних провайдерів чи событий.
 */
export const useFavoritesStore = defineStore('favorites', () => {
  const ids = usePersistedRef<string[]>('b2s_favorites', []);

  const count = computed(() => ids.value.length);

  function isFavorite(workId: string): boolean {
    return ids.value.includes(workId);
  }

  function add(workId: string): void {
    if (!ids.value.includes(workId)) {
      // Робимо новий масив (а не push), щоб watch у usePersistedRef
      // 100% виявив зміну навіть без deep:true.
      ids.value = [...ids.value, workId];
    }
  }

  function remove(workId: string): void {
    ids.value = ids.value.filter((id) => id !== workId);
  }

  function toggle(workId: string): void {
    if (isFavorite(workId)) {
      remove(workId);
    } else {
      add(workId);
    }
  }

  function clear(): void {
    ids.value = [];
  }

  return {
    ids,
    count,
    isFavorite,
    add,
    remove,
    toggle,
    clear,
  };
});
