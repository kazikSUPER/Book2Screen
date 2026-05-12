import { defineStore } from 'pinia';
import { computed } from 'vue';
import * as authApi from '../services/auth';
import type { LoginRequest, RegisterRequest } from '../services/types';
import { usePersistedRef } from '../composables/usePersistedRef';

/**
 * Стор автентифікації + профіль користувача.
 *
 * Persistence через usePersistedRef — сесія і профіль автоматично
 * зберігаються у localStorage між сесіями браузера.
 */
export const useUserStore = defineStore('user', () => {
  // ── Auth ─────────────────────────────────────────────────
  const token = usePersistedRef<string>('b2s_token', '');
  const email = usePersistedRef<string>('b2s_email', '');
  const nickname = usePersistedRef<string>('b2s_nickname', '');
  const userId = usePersistedRef<string>('b2s_userId', '');

  // ── Profile (SCRUM-64) ───────────────────────────────────
  // Опційні поля, що користувач може заповнити у ProfileView.
  const fullName = usePersistedRef<string>('b2s_profile_name', '');
  const birthDate = usePersistedRef<string>('b2s_profile_dob', ''); // ISO-формат YYYY-MM-DD
  const avatarUrl = usePersistedRef<string>('b2s_profile_avatar', '');

  const isAuthenticated = computed(() => !!token.value && !!email.value);

  function setSession(payload: { token: string; userId: string; email: string; nickname: string }): void {
    token.value = payload.token;
    email.value = payload.email;
    nickname.value = payload.nickname;
    userId.value = payload.userId;
  }

  async function login(payload: LoginRequest): Promise<void> {
    const loginResponse = await authApi.login(payload);
    setSession(loginResponse);
  }

  async function register(payload: RegisterRequest): Promise<void> {
    const registerResponse = await authApi.register(payload);
    setSession({
      token: registerResponse.token,
      userId: registerResponse.userId,
      email: registerResponse.email,
      nickname: registerResponse.nickname,
    });
  }

  // SCRUM-24 тФА Підтвердження зміни паролю.
  async function resetPassword(payload: authApi.PasswordResetConfirmRequest): Promise<void> {
    const response = await authApi.confirmPasswordReset(payload);
    setSession(response);
  }

  function logout(): void {
    token.value = '';
    email.value = '';
    nickname.value = '';
    userId.value = '';
    // Профіль (фото, ПІБ, ДН) — лишаємо: при наступному логіні людина
    // не хоче знов заповнювати. Якщо треба — окрема дія "Видалити дані".
  }

  // SCRUM-64 — методи для редагування профілю.
  // Поки бекенд не готовий — пишемо тільки локально. TODO: PUT /api/v1/users/me.
  function updateProfile(patch: { fullName?: string; nickname?: string; birthDate?: string; avatarUrl?: string }): void {
    if (patch.fullName !== undefined) fullName.value = patch.fullName;
    if (patch.nickname !== undefined) nickname.value = patch.nickname;
    if (patch.birthDate !== undefined) birthDate.value = patch.birthDate;
    if (patch.avatarUrl !== undefined) avatarUrl.value = patch.avatarUrl;
  }

  return {
    // auth
    isAuthenticated,
    email,
    nickname,
    userId,
    token,
    login,
    register,
    resetPassword,
    logout,
    // profile
    fullName,
    birthDate,
    avatarUrl,
    updateProfile,
  };
});
