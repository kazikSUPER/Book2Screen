import { apiClient } from './api';
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from './types';

// POST /api/v1/auth/register
// Реєстрація нового користувача.

export async function register(payload: RegisterRequest): Promise<RegisterResponse> {
  const response = await apiClient.post<RegisterResponse>('/api/v1/auth/register', payload);
  return response.data;
}

// POST /api/v1/auth/login
// Авторизація. Повертає JWT-токен і дані користувача.

export async function login(payload: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>('/api/v1/auth/login', payload);
  return response.data;
}

// POST /api/v1/auth/password-reset
// Запит на відновлення паролю (надсилання листа на email).

export async function requestPasswordReset(email: string): Promise<void> {
  await apiClient.post('/api/v1/auth/password-reset', { email });
}

// POST /api/v1/auth/verify-code
// Перевірка коду відновлення.

export async function verifyCode(email: string, code: string): Promise<void> {
  await apiClient.post('/api/v1/auth/verify-code', { email, code });
}

// POST /api/v1/auth/reset-password
// Встановлення нового пароля.

export async function resetPassword(payload: { email: string; code: string; newPassword: string }): Promise<void> {
  await apiClient.post('/api/v1/auth/reset-password', payload);
}
