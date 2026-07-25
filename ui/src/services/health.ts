import { apiClient } from './api';

/**
 * Health-check бекенду.
 *
 * Бек не має окремого /health endpoint (станом на v1 Swagger).
 * Замість нього робимо легкий HEAD/GET на /Works — якщо повертає 200/304,
 * бекенд живий. Це не ідеально, але працює до появи /health.
 */
export interface HealthResponse {
  status: 'UP' | 'DOWN' | string;
}

export async function checkHealth(): Promise<HealthResponse> {
  // GET /api/v1/Works — найдешевший публічний endpoint без авторизації.
  // Якщо бек відповість будь-чим — він "UP".
  await apiClient.get('/api/v1/Works', { timeout: 5000 });
  return { status: 'UP' };
}
