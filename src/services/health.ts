import { apiClient } from './api';

export interface HealthResponse {
  status: 'UP' | 'DOWN' | string;
  db?: string;
  timestamp?: string;
}

/**
 * Перевірка стану бекенду.
 * Endpoint: GET /health (без авторизації).
 */
export async function checkHealth(): Promise<HealthResponse> {
  const response = await apiClient.get<HealthResponse>('/health');
  return response.data;
}
