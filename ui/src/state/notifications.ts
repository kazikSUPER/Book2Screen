import { defineStore } from 'pinia';
import { ref } from 'vue';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface Notification {
  id: number;
  type: NotificationType;
  message: string;
  // Час життя в мс. Якщо null — тост не зникає сам.
  duration: number | null;
}

/**
 * Глобальний стек тостів. Заглядає у нього `<ToastContainer>`
 * (рендериться один раз у App.vue), а додають записи:
 *   1. response-interceptor у api.ts — для 5xx / network / 403,
 *   2. компоненти напряму через push() / pushSuccess().
 *
 * Inline-валідацію (400-ві помилки форм) НЕ дублюємо тостом —
 * хай локальний catch у модалці показує її поряд з полем.
 */
export const useNotificationsStore = defineStore('notifications', () => {
  const items = ref<Notification[]>([]);
  let nextId = 1;

  function push(type: NotificationType, message: string, duration: number | null = 4000): number {
    const id = nextId++;
    items.value = [...items.value, { id, type, message, duration }];

    if (duration !== null) {
      setTimeout(() => remove(id), duration);
    }

    return id;
  }

  function remove(id: number): void {
    items.value = items.value.filter((n) => n.id !== id);
  }

  // Зручні шорткати — щоб у компоненті не передавати тип рядком.
  const pushSuccess = (msg: string, duration?: number | null) => push('success', msg, duration);
  const pushError = (msg: string, duration?: number | null) => push('error', msg, duration ?? 6000);
  const pushWarning = (msg: string, duration?: number | null) => push('warning', msg, duration);
  const pushInfo = (msg: string, duration?: number | null) => push('info', msg, duration);

  return {
    items,
    push,
    remove,
    pushSuccess,
    pushError,
    pushWarning,
    pushInfo,
  };
});
