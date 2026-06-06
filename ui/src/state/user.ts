import { defineStore } from 'pinia';
import { computed } from 'vue';
import * as authApi from '../services/auth';
import * as profileApi from '../services/profile';
import type {
  LoginRequest,
  RegisterRequest,
  ResetPasswordRequest,
  AuthResponse,
  UserProfileDto,
} from '../services/types';
import { usePersistedRef } from '../composables/usePersistedRef';

/**
 * Стор автентифікації + профілю користувача (Book2Screen v1).
 *
 * Зберігаємо у localStorage через usePersistedRef — щоб сесія переживала перезавантаження.
 *
 * Поля з бекенду (AuthResponse): token, userId, email, nickname, role.
 * Поля з бекенду (UserProfileDto): username, email, avatarUrl, joinedAt.
 *
 * УВАГА: на беку немає `birthDate`. Якщо потрібно — додати на бекенді.
 */
export const useUserStore = defineStore('user', () => {
  // ── Auth ─────────────────────────────────────────────────
  const token = usePersistedRef<string>('b2s_token', '');
  const userId = usePersistedRef<string>('b2s_userId', '');
  const email = usePersistedRef<string>('b2s_email', '');
  const username = usePersistedRef<string>('b2s_username', '');
  const role = usePersistedRef<string>('b2s_role', '');

  // ── Профіль (SCRUM-64) — підвантажується з /users/me ─────
  const avatarUrl = usePersistedRef<string>('b2s_profile_avatar', '');
  const joinedAt = usePersistedRef<string>('b2s_profile_joined', '');

  const isAuthenticated = computed(() => !!token.value && !!email.value);
  const isAdmin = computed(() => role.value === 'admin' || role.value === 'moderator');

  function applySession(payload: AuthResponse): void {
    token.value = payload.token;
    userId.value = payload.userId;
    email.value = payload.email;
    username.value = payload.username;
    role.value = payload.role ?? 'user';
  }

  async function login(payload: LoginRequest): Promise<void> {
    const res = await authApi.login(payload);
    applySession(res);
    // Підтягуємо профіль (username/avatar/joinedAt).
    await refreshProfile().catch(() => undefined);
  }

  async function register(payload: RegisterRequest): Promise<void> {
    const res = await authApi.register(payload);
    applySession(res);
    await refreshProfile().catch(() => undefined);
  }

  /** Крок 3 password-reset: ResetPasswordRequest = { email, code, newPassword }. */
  async function resetPassword(payload: ResetPasswordRequest): Promise<void> {
    await authApi.resetPassword(payload);
    // Бек відповідає 200 OK без токена — потрібен явний логін.
    await login({ email: payload.email, password: payload.newPassword });
  }

  function logout(): void {
    token.value = '';
    userId.value = '';
    email.value = '';
    username.value = '';
    role.value = '';
    // Профільні дані лишаємо в localStorage — після наступного login підвантажимо.
  }

  // ── Profile (SCRUM-64) ───────────────────────────────────

  async function refreshProfile(): Promise<void> {
    const profile = await profileApi.fetchMyProfile();
    username.value = profile.username;
    if (profile.avatarUrl !== undefined) avatarUrl.value = profile.avatarUrl;
    joinedAt.value = profile.joinedAt;
  }

  async function updateProfile(patch: Partial<UserProfileDto>): Promise<void> {
    const next: UserProfileDto = {
      username: patch.username ?? username.value,
      email: patch.email ?? email.value,
      avatarUrl: patch.avatarUrl ?? avatarUrl.value,
      joinedAt: patch.joinedAt ?? (joinedAt.value || new Date().toISOString()),
    };
    await profileApi.updateMyProfile(next);
    username.value = next.username;
    if (next.avatarUrl !== undefined) avatarUrl.value = next.avatarUrl;
  }

  async function setAvatar(url: string): Promise<void> {
    await profileApi.updateMyAvatar(url);
    avatarUrl.value = url;
  }

  return {
    // auth
    isAuthenticated,
    isAdmin,
    token,
    userId,
    email,
    username,
    role,
    login,
    register,
    resetPassword,
    logout,
    // profile
    avatarUrl,
    joinedAt,
    refreshProfile,
    updateProfile,
    setAvatar,
  };
});
