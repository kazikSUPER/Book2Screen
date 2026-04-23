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

const email = ref('');
const password = ref('');
const errors = ref({ email: '', password: '' });
const apiError = ref('');
const isSubmitting = ref(false);

const validate = (): boolean => {
  errors.value = { email: '', password: '' };
  let isValid = true;

  if (!email.value) {
    errors.value.email = 'Введіть email';
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
    await userStore.login({ email: email.value, password: password.value });
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
          <label class="field-label">Email</label>
          <input
            v-model="email"
            type="email"
            class="field-input"
            :class="{ error: errors.email }"
            placeholder="Введіть email"
            :disabled="isSubmitting"
          />
          <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
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
  background: rgba(0, 0, 0, 0.57);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  font-family: 'Inter', sans-serif;
}

.modal {
  background-color: #f7cccc;
  outline: 1px black solid;
  padding: 50px 70px;
  display: flex;
  flex-direction: column;
  gap: 18px;
  position: relative;
  width: 550px;
  max-width: 95vw;
  border-radius: 5px;
  box-shadow: 5px 4px 4px rgba(0, 0, 0, 0.25);
}

.modal-close {
  position: absolute;
  top: 12px;
  right: 16px;
  background: none;
  border: none;
  font-size: 20px;
  color: black;
  cursor: pointer;
  line-height: 1;
  font-weight: 700;
}

.modal-close:hover {
  color: #8e182f;
}

.modal-title {
  font-size: 28px;
  font-weight: 400;
  color: black;
  margin: 0;
  text-align: center;
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field-label {
  font-size: 18px;
  font-weight: 400;
  color: black;
}

.field-input {
  width: 100%;
  height: 44px;
  padding: 0 10px;
  background-color: #f7cccc;
  border: 1px black solid;
  border-radius: 5px;
  color: black;
  font-size: 16px;
  outline: none;
  box-sizing: border-box;
}

.field-input:focus {
  border-color: #8e182f;
  border-width: 2px;
}

.field-input.error {
  border-color: #cc0000;
  border-width: 2px;
}

.field-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error-text {
  color: #cc0000;
  font-size: 13px;
}

.api-error {
  background: rgba(204, 0, 0, 0.1);
  border: 1px solid #cc0000;
  color: #cc0000;
  padding: 8px 12px;
  border-radius: 4px;
  font-size: 14px;
  margin: 0;
  text-align: center;
}

.forgot-link {
  text-align: right;
  font-size: 16px;
  color: #4e1bda;
  cursor: pointer;
  margin: 0;
  text-decoration: underline;
}

.forgot-link:hover {
  color: #2a00a8;
}

.btn-row {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 4px;
}

.btn-login,
.btn-register {
  height: 44px;
  background-color: #3d0000;
  color: white;
  border: 1px black solid;
  border-radius: 5px;
  padding: 5px;
  font-size: 18px;
  font-weight: 400;
  cursor: pointer;
  box-shadow: 0px 4px 4px rgba(0, 0, 0, 0.25);
  transition: background 0.2s;
}

.btn-login {
  width: 140px;
}

.btn-register {
  width: 200px;
}

.btn-login:hover:not(:disabled),
.btn-register:hover:not(:disabled) {
  background-color: #5a0000;
}

.btn-login:disabled,
.btn-register:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
