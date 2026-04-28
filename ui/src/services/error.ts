import axios, { AxiosError } from 'axios';
import type { ApiError } from './types';

// Витягує людино-читабельне повідомлення з помилки axios.
// Backend повертає JSON у форматі { timestamp, errorCode, message, path, details? }
// (контракт Етапу 2, крок 3 Етапу 3: Global Exception Handler).

export function extractErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const axiosErr = err as AxiosError<ApiError>;

    // 1) Стандартизована помилка від нашого Global Exception Handler
    if (axiosErr.response?.data?.message) {
      return axiosErr.response.data.message;
    }

    // 2) Мережа / бекенд не відповідає
    if (axiosErr.code === 'ERR_NETWORK') {
      return 'Сервер недоступний. Перевірте підключення до backend.';
    }

    // 3) Стандартні HTTP-помилки без тіла
    if (axiosErr.response?.status) {
      return `Помилка сервера (${axiosErr.response.status})`;
    }
  }

  if (err instanceof Error) return err.message;
  return 'Невідома помилка';
}
