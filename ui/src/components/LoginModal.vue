<script setup lang="ts">
import { ref } from 'vue';
import { useUserStore } from '../state/user';
import { extractErrorMessage } from '../services/error';

const emit = defineEmits<{
  close: [];
  openRegister: [];
  openReset: [];
}>();

const userStore = useUserStore();

const login = ref('');
const password = ref('');
const errors = ref({ login: '', password: '' });
const apiError = ref('');
const isSubmitting = ref(false);

const validate = (): boolean => {
  errors.value = { login: '', password: '' };
  let isValid = true;

  if (!login.value) {
    errors.value.login = 'Введіть логін';
    isValid = false;
  }

  if (!password.value) {
    errors.value.password = 'Введіть пароль';
    isValid = false;
  }

  return isValid;
};

const handleLogin = async () => {
  if (!validate()) return;
  apiError.value = '';
  isSubmitting.value = true;
  try {
    // У backend-контракті поле називається email (див. services/types.ts).
    // Користувач може вводити сюди свій email — передаємо як є.
    await userStore.login({ email: login.value, password: password.value });
    emit('close');
  } catch (err) {
    apiError.value = extractErrorMessage(err);
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal">
      <button class="modal-close" @click="emit('close')">✕</button>

      <h2 class="modal-title">Вхід</h2>

      <div class="modal-body">
        <div class="field">
          <label class="field-label">Логін</label>
          <input
            v-model="login"
            type="text"
            class="field-input"
            :class="{ error: errors.login }"
            placeholder="Введіть логін"
            :disabled="isSubmitting"
          />
          <span v-if="errors.login" class="error-text">{{ errors.login }}</span>
        </div>

        <div class="field">
          <label class="field-label">Пароль</label>
          <input
            v-model="password"
            type="password"
            class="field-input"
            :class="{ error: errors.password }"
            placeholder="Введіть пароль"
            :disabled="isSubmitting"
          />
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <p v-if="apiError" class="api-error">{{ apiError }}</p>

        <p class="forgot-link" @click="emit('openReset')">Забули пароль?</p>

        <div class="btn-row">
          <button class="btn-login" :disabled="isSubmitting" @click="handleLogin">
            {{ isSubmitting ? 'Вхід...' : 'Ввійти' }}
          </button>
          <button class="btn-register" :disabled="isSubmitting" @click="emit('openRegister')">Зареєструватись</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background-color: var(--pink-light);
  border-radius: 12px;
  width: 100%;
  max-width: 380px;
  padding: 32px;
  position: relative;
}

.modal-close {
  position: absolute;
  top: 12px;
  right: 16px;
  background: none;
  border: none;
  font-size: 18px;
  color: var(--dark-card);
  cursor: pointer;
}

.modal-close:hover {
  color: var(--accent);
}

.modal-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--dark-card);
  font-family: 'Georgia', serif;
  margin: 0 0 24px;
  text-align: center;
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.field-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--dark-card);
  font-family: 'Georgia', serif;
}

.field-input {
  background-color: #fff;
  border: 1px solid var(--pink-mid);
  border-radius: 6px;
  padding: 10px 14px;
  color: var(--dark-card);
  font-size: 14px;
  font-family: 'Georgia', serif;
  outline: none;
  transition: border-color 0.2s;
}

.field-input::placeholder {
  color: #aaa;
}

.field-input:focus {
  border-color: var(--accent);
}

.field-input.error {
  border-color: #ff6b6b;
}

.field-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error-text {
  color: #cc0000;
  font-size: 12px;
}

.api-error {
  color: #cc0000;
  font-size: 13px;
  text-align: center;
  padding: 6px 8px;
  background: rgba(204, 0, 0, 0.08);
  border-radius: 4px;
  margin: 0;
}

.forgot-link {
  text-align: right;
  font-size: 13px;
  color: var(--accent);
  cursor: pointer;
  margin: 0;
  text-decoration: underline;
}

.forgot-link:hover {
  color: var(--dark-card);
}

.btn-row {
  display: flex;
  gap: 10px;
  margin-top: 4px;
}

.btn-login {
  flex: 1;
  background-color: var(--accent);
  color: var(--pink-light);
  border: none;
  border-radius: 6px;
  padding: 12px;
  font-size: 15px;
  font-family: 'Georgia', serif;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-login:hover:not(:disabled) {
  background-color: #a82040;
}

.btn-login:disabled,
.btn-register:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-register {
  flex: 1;
  background-color: var(--dark-card);
  color: var(--pink-light);
  border: none;
  border-radius: 6px;
  padding: 12px;
  font-size: 15px;
  font-family: 'Georgia', serif;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-register:hover:not(:disabled) {
  background-color: #5a0000;
}
</style>
