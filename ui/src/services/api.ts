import axios, { AxiosError } from 'axios';
import { useUserStore } from '../state/user';
import { useNotificationsStore } from '../state/notifications';
import type { ApiError } from './types';
import { API_URL } from './env';
import { STR } from '../constants';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// ── Request: автоматично додаємо Authorization ────────────────
apiClient.interceptors.request.use(
  (config) => {
    const userStore = useUserStore();
    if (userStore.token) {
      config.headers.Authorization = `Bearer ${userStore.token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Response: глобальна обробка типових помилок ───────────────
//
// Розподіл відповідальності:
//   • 401            — авто-logout (токен прострочений / невалідний)
//   • 403            — toast «Немає доступу»
//   • 5xx або network— toast «Сервер недоступний / помилка сервера»
//   • 4xx (інші)     — НЕ показуємо тост. Хай локальний catch у компоненті
//                      (LoginModal, форма голосу тощо) виведе inline помилку
//                      поряд з полем — це валідація, тост дублював би її.
//
// Помилка завжди прокидається далі (Promise.reject), щоб локальний код
// міг встановити свій errorMessage / зробити retry і тд.
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiError>) => {
    const status = error.response?.status;

    // 1) Невалідний токен — кікаємо.
    if (status === 401) {
      const userStore = useUserStore();
      const notifications = useNotificationsStore();
      if (userStore.isAuthenticated) {
        userStore.logout();
        notifications.pushWarning(STR.common.sessionExpired);
      }
      return Promise.reject(error);
    }

    // 2) Заборонено.
    if (status === 403) {
      const notifications = useNotificationsStore();
      const message = error.response?.data?.message || STR.common.accessDenied;
      notifications.pushError(message);
      return Promise.reject(error);
    }

    // 3) Серверні помилки і мережа.
    if ((status && status >= 500) || error.code === 'ERR_NETWORK') {
      const notifications = useNotificationsStore();
      const message =
        error.code === 'ERR_NETWORK'
          ? STR.common.networkError
          : error.response?.data?.message || `Помилка сервера (${status})`;
      notifications.pushError(message);
      return Promise.reject(error);
    }

    // 4) 4xx (валідація, конфлікти, not-found) — нехай вирішує компонент.
    return Promise.reject(error);
  }
);
