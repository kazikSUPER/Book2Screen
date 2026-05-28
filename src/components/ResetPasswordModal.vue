<script setup lang="ts">
import { ref, computed } from 'vue';
import * as authApi from '../services/auth';
import { extractErrorMessage } from '../services/error';
import { STR } from '../constants';

const emit = defineEmits<{
  close: [];
  success: [];
}>();

const t = STR.auth;

const email = ref('');
const code = ref('');
const codeSent = ref(false);
const emailError = ref('');
const codeError = ref('');
const apiError = ref('');
const isSendingCode = ref(false);
const isSubmitting = ref(false);

const isEmailValid = computed(() => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value));

const handleSendCode = async () => {
  if (!isEmailValid.value) {
    emailError.value = t.invalidEmail;
    return;
  }
  emailError.value = '';
  apiError.value = '';
  isSendingCode.value = true;
  try {
    await authApi.requestPasswordReset(email.value);
    codeSent.value = true;
  } catch (err) {
    apiError.value = extractErrorMessage(err);
  } finally {
    isSendingCode.value = false;
  }
};

const handleReset = async () => {
  codeError.value = '';
  if (!code.value) {
    codeError.value = t.emptyCode;
    return;
  }

  apiError.value = '';
  isSubmitting.value = true;
  try {
    await authApi.confirmPasswordReset({
      email: email.value,
      code: code.value.trim(),
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
    <div class="modal" role="dialog" aria-modal="true" aria-labelledby="reset-title">
      <button class="modal-close" type="button" :aria-label="STR.common.close" @click="emit('close')">✕</button>

      <h2 id="reset-title" class="modal-title">{{ t.resetTitle }}</h2>

      <div class="modal-body">
        <div class="field">
          <label class="field-label">{{ t.resetEmailLabel }}</label>
          <div class="email-row">
            <input
              v-model="email"
              type="email"
              autocomplete="email"
              class="field-input"
              :class="{ error: emailError }"
              :placeholder="t.emailPlaceholder"
              :disabled="codeSent || isSendingCode"
            />
            <button
              type="button"
              class="send-btn"
              :disabled="!isEmailValid || isSendingCode || codeSent"
              @click="handleSendCode"
            >
              {{ isSendingCode ? t.sending : t.sendCode }}
            </button>
          </div>
          <span v-if="emailError" class="error-text">{{ emailError }}</span>
          <span v-if="codeSent" class="success-text">{{ t.codeSentTo(email) }}</span>
          <span v-if="apiError && !codeSent" class="error-text">{{ apiError }}</span>
        </div>

        <div class="field">
          <label class="field-label">{{ t.codeLabel }}</label>
          <input
            v-model="code"
            type="text"
            class="field-input"
            :class="{ error: codeError }"
            :placeholder="t.codePlaceholder"
            :disabled="isSubmitting"
            autocomplete="one-time-code"
          />
          <span v-if="codeError" class="error-text">{{ codeError }}</span>
        </div>

        <p v-if="apiError && codeSent" class="api-error">{{ apiError }}</p>

        <button type="button" class="reset-btn" :disabled="isSubmitting" @click="handleReset">
          {{ isSubmitting ? t.submittingReset : t.submitReset }}
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
  gap: 6px;
}

.field-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-on-light);
  font-family: var(--font-display);
}

.email-row {
  display: flex;
  gap: 8px;
}

.field-input {
  flex: 1;
  background-color: var(--color-input-bg);
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  padding: 10px 14px;
  color: var(--text-on-light);
  font-size: 14px;
  font-family: var(--font-body);
  outline: none;
  transition: border-color 0.2s;
  min-width: 0;
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

.send-btn {
  background-color: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-sm);
  padding: 10px 12px;
  font-size: 13px;
  font-family: var(--font-display);
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.2s;
  flex-shrink: 0;
}

.send-btn:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.send-btn:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

.error-text {
  color: var(--text-error);
  font-size: 12px;
  font-family: var(--font-body);
}

.success-text {
  color: #2a7a2a;
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

.reset-btn {
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

.reset-btn:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}

.reset-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.reset-btn:focus-visible {
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

  .email-row {
    flex-direction: column;
  }
}
</style>
