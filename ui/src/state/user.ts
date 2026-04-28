import { defineStore } from 'pinia';
import { ref } from 'vue';
import * as authApi from '../services/auth';
import type { LoginRequest, RegisterRequest } from '../services/types';

export const useUserStore = defineStore('user', () => {
  const isAuthenticated = ref(false);
  const email = ref('');
  const nickname = ref('');
  const userId = ref('');
  const token = ref('');

  // Відновлюємо сесію з localStorage при завантаженні сторінки
  const savedToken = localStorage.getItem('b2s_token');
  const savedEmail = localStorage.getItem('b2s_email');
  const savedNickname = localStorage.getItem('b2s_nickname');
  const savedUserId = localStorage.getItem('b2s_userId');
  if (savedToken && savedEmail) {
    token.value = savedToken;
    email.value = savedEmail;
    nickname.value = savedNickname || '';
    userId.value = savedUserId || '';
    isAuthenticated.value = true;
  }

  // Реальний логін через backend API.
  // Кидає помилку — модалка її перехопить і покаже користувачу.
  async function login(payload: LoginRequest): Promise<void> {
    const data = await authApi.login(payload);
    token.value = data.token;
    email.value = data.email;
    nickname.value = data.nickname;
    userId.value = data.userId;
    isAuthenticated.value = true;

    localStorage.setItem('b2s_token', data.token);
    localStorage.setItem('b2s_email', data.email);
    localStorage.setItem('b2s_nickname', data.nickname);
    localStorage.setItem('b2s_userId', data.userId);
  }

  // Реєстрація через backend API.
  // Після успішної реєстрації автоматично логінимось.

  async function register(payload: RegisterRequest): Promise<void> {
    await authApi.register(payload);
    await login({ email: payload.email, password: payload.password });
  }

  function logout(): void {
    isAuthenticated.value = false;
    email.value = '';
    nickname.value = '';
    userId.value = '';
    token.value = '';

    localStorage.removeItem('b2s_token');
    localStorage.removeItem('b2s_email');
    localStorage.removeItem('b2s_nickname');
    localStorage.removeItem('b2s_userId');
  }

  return {
    isAuthenticated,
    email,
    nickname,
    userId,
    token,
    login,
    register,
    logout,
  };
});
