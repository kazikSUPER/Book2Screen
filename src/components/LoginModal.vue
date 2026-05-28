<script setup lang="ts">
import { ref } from 'vue';
import { useUserStore } from '../state/user';
import { extractErrorMessage } from '../services/error';
import { STR } from '../constants';

const emit = defineEmits<{
  close: [];
  openRegister: [];
  openReset: [];
}>();

const userStore = useUserStore();
const t = STR.auth;

const login = ref('');
const password = ref('');
const errors = ref({ login: '', password: '' });
const apiError = ref('');
const isSubmitting = ref(false);

const validate = (): boolean => {
  errors.value = { login: '', password: '' };
  let isValid = true;

  if (!login.value) {
    errors.value.login = t.emptyLogin;
    isValid = false;
  }

  if (!password.value) {
    errors.value.password = t.emptyPassword;
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
    <div class="modal" role="dialog" aria-modal="true" aria-labelledby="login-title">
      <button class="modal-close" type="button" :aria-label="STR.common.close" @click="emit('close')">✕</button>

      <h2 id="login-title" class="modal-title">{{ t.loginTitle }}</h2>

      <div class="modal-body">
        <div class="field">
          <label class="field-label">{{ t.login }}</label>
          <input
            v-model="login"
            type="text"
            class="field-input"
            :class="{ error: errors.login }"
            :placeholder="t.loginPlaceholder"
            :disabled="isSubmitting"
          />
          <span v-if="errors.login" class="error-text">{{ errors.login }}</span>
        </div>

        <div class="field">
          <label class="field-label">{{ t.password }}</label>
          <input
            v-model="password"
            type="password"
            class="field-input"
            :class="{ error: errors.password }"
            :placeholder="t.passwordPlaceholder"
            :disabled="isSubmitting"
          />
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <p v-if="apiError" class="api-error">{{ apiError }}</p>

        <button type="button" class="forgot-link" @click="emit('openReset')">
          {{ t.forgotPassword }}
        </button>

        <div class="btn-row">
          <button type="button" class="btn-login" :disabled="isSubmitting" @click="handleLogin">
            {{ isSubmitting ? t.submitting : t.submitLogin }}
          </button>
          <button type="button" class="btn-register" :disabled="isSubmitting" @click="emit('openRegister')">
            {{ t.submitRegister }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(49, 22, 32, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  padding: 16px;
}

.modal {
  background-color: var(--color-modal-bg);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  width: 100%;
  max-width: 380px;
  padding: 32px;
  position: relative;
  box-shadow: var(--shadow-md);
}

.modal-close {
  position: absolute;
  top: 12px;
  right: 16px;
  background: none;
  border: none;
  font-size: 18px;
  color: var(--text-on-light);
  cursor: pointer;
  line-height: 1;
}

.modal-close:hover {
  color: var(--color-primary);
}

.modal-close:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
  border-radius: var(--radius-xs);
}

.modal-title {
  font-size: 22px;
  font-weight: 400;
  color: var(--text-on-light);
  font-family: var(--font-display);
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
  font-weight: 500;
  color: var(--text-on-light);
  font-family: var(--font-display);
}

.field-input {
  background-color: var(--color-input-bg);
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  padding: 10px 14px;
  color: var(--text-on-light);
  font-size: 14px;
  font-family: var(--font-body);
  outline: none;
  transition: border-color 0.2s;
}

.field-input::placeholder {
  color: var(--text-muted);
}

.field-input:focus {
  border-color: var(--color-primary);
}

.field-input.error {
  border-color: var(--text-error);
}

.field-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error-text {
  color: var(--text-error);
  font-size: 12px;
  font-family: var(--font-body);
}

.api-error {
  color: var(--text-error);
  font-size: 13px;
  text-align: center;
  padding: 6px 8px;
  background: rgba(198, 40, 40, 0.08);
  border-radius: var(--radius-xs);
  margin: 0;
  font-family: var(--font-body);
}

.forgot-link {
  align-self: flex-end;
  background: none;
  border: none;
  font-size: 13px;
  color: var(--color-primary);
  cursor: pointer;
  padding: 0;
  font-family: var(--font-display);
  text-decoration: underline;
}

.forgot-link:hover {
  color: var(--color-primary-hover);
}

.forgot-link:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

.btn-row {
  display: flex;
  gap: 10px;
  margin-top: 4px;
}

.btn-login,
.btn-register {
  flex: 1;
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-sm);
  padding: 10px;
  font-size: 14px;
  font-family: var(--font-display);
  font-weight: 400;
  cursor: pointer;
  color: var(--text-on-primary);
  transition: background 0.2s;
}

.btn-login {
  background-color: var(--color-primary);
}

.btn-login:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}

.btn-register {
  background-color: var(--color-card);
  border-color: var(--color-card);
}

.btn-register:hover:not(:disabled) {
  background-color: var(--color-primary);
  border-color: var(--color-primary-dark);
}

.btn-login:disabled,
.btn-register:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-login:focus-visible,
.btn-register:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

/* ── Адаптив ── */
@media (max-width: 480px) {
  .modal {
    max-width: calc(100vw - 24px);
    padding: 24px 20px;
  }

  .modal-title {
    font-size: 18px;
    margin-bottom: 18px;
  }

  .btn-row {
    flex-direction: column;
    gap: 8px;
  }
}
</style>
