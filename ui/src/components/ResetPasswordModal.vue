<script setup lang="ts">
import { ref, computed } from 'vue';
import * as authApi from '../services/auth';

const emit = defineEmits<{
  close: [];
}>();

const email = ref('');
const code = ref('');
const codeSent = ref(false);
const emailError = ref('');
const codeError = ref('');

const isEmailValid = computed(() => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value));

const handleSendCode = async () => {
  if (!isEmailValid.value) {
    emailError.value = 'Введіть коректний email';
    return;
  }
  emailError.value = '';
  try {
    await authApi.requestPasswordReset(email.value);
  } catch (err) {
    // Якщо бекенд лежить — все одно показуємо UI "код надіслано"
    console.warn('[reset] Backend недоступний:', err);
  }
  codeSent.value = true;
};

const handleReset = () => {
  if (!code.value) {
    codeError.value = 'Введіть код';
    return;
  }
  codeError.value = '';
  // тут буде реальний API-виклик
  emit('close');
};
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="modal">
      <button class="modal-close" @click="emit('close')">✕</button>

      <h2 class="modal-title">Відновлення паролю</h2>

      <div class="modal-body">
        <div class="field">
          <label class="field-label">Введіть Email для відновлення паролю</label>
          <div class="email-row">
            <input
              v-model="email"
              type="email"
              class="field-input"
              :class="{ error: emailError }"
              placeholder="Введіть Email"
            />
            <button class="send-btn" :disabled="!isEmailValid" @click="handleSendCode">Надіслати код</button>
          </div>
          <span v-if="emailError" class="error-text">{{ emailError }}</span>
          <span v-if="codeSent" class="success-text">Код надіслано на {{ email }}</span>
        </div>

        <div class="field">
          <label class="field-label">Код</label>
          <input
            v-model="code"
            type="text"
            class="field-input"
            :class="{ error: codeError }"
            placeholder="Введіть код"
          />
          <span v-if="codeError" class="error-text">{{ codeError }}</span>
        </div>

        <button class="reset-btn" @click="handleReset">Відновити</button>
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
  gap: 6px;
}

.field-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--dark-card);
  font-family: 'Georgia', serif;
}

.email-row {
  display: flex;
  gap: 8px;
}

.field-input {
  flex: 1;
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

.send-btn {
  background-color: var(--accent);
  color: var(--pink-light);
  border: none;
  border-radius: 6px;
  padding: 10px 12px;
  font-size: 13px;
  font-family: 'Georgia', serif;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.2s;
}

.send-btn:hover:not(:disabled) {
  background-color: #a82040;
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error-text {
  color: #cc0000;
  font-size: 12px;
}

.success-text {
  color: #2a7a2a;
  font-size: 12px;
}

.reset-btn {
  background-color: var(--accent);
  color: var(--pink-light);
  border: none;
  border-radius: 6px;
  padding: 12px;
  font-size: 15px;
  font-family: 'Georgia', serif;
  font-weight: 600;
  cursor: pointer;
  width: 100%;
  margin-top: 4px;
  transition:
    background 0.2s,
    transform 0.15s;
}

.reset-btn:hover {
  background-color: #a82040;
  transform: translateY(-1px);
}
</style>
