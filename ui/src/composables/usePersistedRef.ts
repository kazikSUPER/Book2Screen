import { ref, watch, type Ref } from 'vue';

/**
 * Реактивна змінна, яка автоматично синхронізується з Web Storage.
 *
 * — При створенні читає значення з storage (або повертає `initial`).
 * — При будь-якій зміні `.value` пише оновлене значення назад у storage.
 * — `null` або `undefined` видаляють ключ зі storage.
 *
 * Використання:
 *   const cart   = usePersistedRef<CartItem[]>('b2s_cart', [], sessionStorage);
 *   const token  = usePersistedRef<string | null>('b2s_token', null);
 */
export function usePersistedRef<T>(key: string, initial: T, storage: Storage = localStorage): Ref<T> {
  // 1. Намагаємось прочитати збережене значення.
  let initialValue: T = initial;
  try {
    const raw = storage.getItem(key);
    if (raw !== null) {
      initialValue = JSON.parse(raw) as T;
    }
  } catch (err) {
    // Якщо JSON битий — стираємо ключ і використовуємо default.
    console.warn(`[usePersistedRef] Не вдалось прочитати "${key}":`, err);
    storage.removeItem(key);
  }

  const r = ref<T>(initialValue) as Ref<T>;

  // 2. Слідкуємо за змінами і пишемо у storage.
  // `deep: true` — щоб працювало і для масивів/обʼєктів (push, splice, ...).
  watch(
    r,
    (newValue) => {
      try {
        if (newValue === null || newValue === undefined) {
          storage.removeItem(key);
        } else {
          storage.setItem(key, JSON.stringify(newValue));
        }
      } catch (err) {
        console.warn(`[usePersistedRef] Не вдалось зберегти "${key}":`, err);
      }
    },
    { deep: true }
  );

  return r;
}
