<script setup lang="ts">
import { ref, computed } from 'vue';
import { useUserStore } from '../state/user';
import { extractErrorMessage } from '../services/error';

const emit = defineEmits<{
  close: [];
  success: [];
}>();

const userStore = useUserStore();

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
    errors.value.email = 'Введіть коректний email';
    isValid = false;
  }

  if (password.value.length < 8) {
    errors.value.password = 'Пароль має бути не менше 8 символів';
    isValid = false;
  }

  if (password.value !== passwordConfirm.value) {
    errors.value.passwordConfirm = 'Паролі не співпадають';
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
    <div class="modal">
      <button class="modal-close" @click="emit('close')">✕</button>

      <h2 class="modal-title">Реєстрація</h2>

      <div class="modal-body">
        <div class="field">
          <label class="field-label">Email</label>
          <input
            v-model="email"
            type="email"
            class="field-input"
            :class="{ error: errors.email }"
            placeholder="Введіть Email"
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

        <div class="field">
          <label class="field-label">Повторіть пароль</label>
          <input
            v-model="passwordConfirm"
            type="password"
            class="field-input"
            :class="{ error: errors.passwordConfirm }"
            placeholder="Введіть пароль"
            :disabled="isSubmitting"
          />
          <span v-if="errors.passwordConfirm" class="error-text">{{ errors.passwordConfirm }}</span>
        </div>

        <p v-if="apiError" class="api-error">{{ apiError }}</p>

        <button class="register-btn" :disabled="!isFormValid || isSubmitting" @click="handleRegister">
          {{ isSubmitting ? 'Реєстрація...' : 'Зареєструватись' }}
        </button>
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
  line-height: 1;
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

.register-btn {
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

.register-btn:hover:not(:disabled) {
  background-color: #a82040;
  transform: translateY(-1px);
}

.register-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
