import { apiClient } from './api';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  PasswordResetConfirmRequest,
  PasswordResetConfirmResponse,
} from './types';

export type { PasswordResetConfirmRequest, PasswordResetConfirmResponse };

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

// POST /api/v1/auth/reset-password
// Підтвердження коду + встановлення нового паролю.
// Повертає дані сесії → одразу авторизує користувача.

export async function confirmPasswordReset(
  payload: PasswordResetConfirmRequest,
): Promise<PasswordResetConfirmResponse> {
  const response = await apiClient.post<PasswordResetConfirmResponse>(
    '/api/v1/auth/reset-password',
    payload,
  );
  return response.data;
}
