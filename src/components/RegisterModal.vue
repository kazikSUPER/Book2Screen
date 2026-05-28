<script setup lang="ts">
import { ref, computed } from 'vue';
import { useUserStore } from '../state/user';
import { extractErrorMessage } from '../services/error';
import { STR } from '../constants';

const emit = defineEmits<{
  close: [];
  success: [];
}>();

const userStore = useUserStore();
const t = STR.auth;

const email = ref('');
const password = ref('');
const passwordConfirm = ref('');

const errors = ref({
  email: '',
  password: '',
  passwordConfirm: '',
});
const apiError = ref('');
const isSubmitting = ref(false);

const isFormValid = computed(() => {
  return (
    /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value) &&
    password.value.length >= 8 &&
    password.value === passwordConfirm.value
  );
});

const validate = (): boolean => {
  errors.value = { email: '', password: '', passwordConfirm: '' };
  let isValid = true;

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
    errors.value.email = t.invalidEmail;
    isValid = false;
  }

  if (password.value.length < 8) {
    errors.value.password = t.passwordTooShort;
    isValid = false;
  }

  if (password.value !== passwordConfirm.value) {
    errors.value.passwordConfirm = t.passwordsMismatch;
    isValid = false;
  }

  return isValid;
};

const handleRegister = async () => {
  if (!validate()) return;
  apiError.value = '';
  isSubmitting.value = true;
  try {
    // nickname формуємо з email-prefix (до @), бо у формі його немає.
    // Якщо бекенд вимагатиме унікальний nickname — згенеруй форму сам або хай бекенд бере з email.
    const nickname = email.value.split('@')[0];
    await userStore.register({
      email: email.value,
      nickname,
      password: password.value,
    });
    emit('success');
  } catch (err) {
    apiError.value = extractErrorMessage(err);
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal" role="dialog" aria-modal="true" aria-labelledby="register-title">
      <button class="modal-close" type="button" :aria-label="STR.common.close" @click="emit('close')">✕</button>

      <h2 id="register-title" class="modal-title">{{ t.registerTitle }}</h2>

      <div class="modal-body">
        <div class="field">
          <label class="field-label">{{ t.email }}</label>
          <input
            v-model="email"
            type="email"
            autocomplete="email"
            class="field-input"
            :class="{ error: errors.email }"
            :placeholder="t.emailPlaceholder"
            :disabled="isSubmitting"
          />
          <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
        </div>

        <div class="field">
          <label class="field-label">{{ t.password }}</label>
          <input
            v-model="password"
            type="password"
            autocomplete="new-password"
            class="field-input"
            :class="{ error: errors.password }"
            :placeholder="t.passwordPlaceholder"
            :disabled="isSubmitting"
          />
          <span v-if="errors.password" class="error-text">{{ errors.password }}</span>
        </div>

        <div class="field">
          <label class="field-label">{{ t.passwordRepeat }}</label>
          <input
            v-model="passwordConfirm"
            type="password"
            autocomplete="new-password"
            class="field-input"
            :class="{ error: errors.passwordConfirm }"
            :placeholder="t.passwordPlaceholder"
            :disabled="isSubmitting"
          />
          <span v-if="errors.passwordConfirm" class="error-text">{{ errors.passwordConfirm }}</span>
        </div>

        <p v-if="apiError" class="api-error">{{ apiError }}</p>

        <button type="button" class="register-btn" :disabled="!isFormValid || isSubmitting" @click="handleRegister">
          {{ isSubmitting ? t.submittingRegister : t.submitRegister }}
        </button>
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

.register-btn {
  background-color: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-sm);
  padding: 12px;
  font-size: 14px;
  font-family: var(--font-display);
  font-weight: 400;
  cursor: pointer;
  width: 100%;
  margin-top: 4px;
  transition: background 0.2s;
}

.register-btn:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}

.register-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.register-btn:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

@media (max-width: 480px) {
  .modal {
    max-width: calc(100vw - 24px);
    padding: 24px 20px;
  }

  .modal-title {
    font-size: 18px;
    margin-bottom: 18px;
  }
}
</style>
