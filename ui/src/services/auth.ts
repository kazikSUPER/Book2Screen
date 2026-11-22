import { apiClient } from './api';
import type {
  LoginRequest,
  AuthResponse,
  RegisterRequest,
  ForgotPasswordRequest,
  VerifyCodeRequest,
  ResetPasswordRequest,
} from './types';

/**
 * Auth API (Book2Screen v1).
 *
 * Бек використовує PascalCase: /api/v1/Auth/...
 * Password-reset тепер 3-крокова:
 *   1) POST /Auth/password-reset  — надіслати email, бек висилає код
 *   2) POST /Auth/verify-code     — перевірити код (опційно перед кроком 3)
 *   3) POST /Auth/reset-password  — встановити новий пароль
 */

// POST /api/v1/Auth/login
export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/v1/Auth/login', payload);
  return response.data;
}

// POST /api/v1/Auth/register
export async function register(payload: RegisterRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/v1/Auth/register', payload);
  return response.data;
}

// POST /api/v1/Auth/password-reset — крок 1: бек висилає 6-значний код на email.
export async function requestPasswordReset(email: string): Promise<void> {
  const payload: ForgotPasswordRequest = { email };
  await apiClient.post('/api/v1/Auth/password-reset', payload);
}

// POST /api/v1/Auth/verify-code — крок 2: перевірити чи код валідний.
// Не змінює пароль! Використовуй, щоб перед "Підтвердити" дати юзеру feedback.
export async function verifyResetCode(payload: VerifyCodeRequest): Promise<void> {
  await apiClient.post('/api/v1/Auth/verify-code', payload);
}

// POST /api/v1/Auth/reset-password — крок 3: встановити новий пароль.
// Бек: newPassword мін. 6 символів.
export async function resetPassword(payload: ResetPasswordRequest): Promise<void> {
  await apiClient.post('/api/v1/Auth/reset-password', payload);
}

// Backward-compat: legacy ім'я, яке state/user.ts ще міг використовувати.
// Семантично робить крок 3, якщо передати { email, code, newPassword }.
export async function confirmPasswordReset(payload: ResetPasswordRequest): Promise<void> {
  return resetPassword(payload);
}
