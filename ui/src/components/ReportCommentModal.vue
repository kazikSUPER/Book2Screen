<script setup lang="ts">
import { ref } from 'vue';
import { reportReview } from '../services/reviews';
import { useNotificationsStore } from '../state/notifications';
import { extractErrorMessage } from '../services/error';
import { REPORT_REASONS, STR } from '../constants';

/**
 * Модалка "Поскаржитись на коментар" (Figma — DetailView, скрін 5).
 *
 * Відкривається при кліку "Поскаржитись" у ReviewItem.
 * Користувач обирає причину з dropdown і натискає "Надіслати".
 * Бекенд: POST /api/v1/reviews/:id/report (або mock).
 */

const props = defineProps<{
  reviewId: string;
}>();

const emit = defineEmits<{
  close: [];
  reported: [];
}>();

const notifications = useNotificationsStore();
const t = STR.report;
const reasons = REPORT_REASONS;

const selectedReason = ref<string>('');
const isSubmitting = ref(false);
const errorMessage = ref('');

async function submit(): Promise<void> {
  if (!selectedReason.value) {
    errorMessage.value = t.reasonRequired;
    return;
  }
  isSubmitting.value = true;
  errorMessage.value = '';
  try {
    await reportReview(props.reviewId, selectedReason.value);
    notifications.pushSuccess(t.submitted);
    emit('reported');
    emit('close');
  } catch (err) {
    errorMessage.value = extractErrorMessage(err);
  } finally {
    isSubmitting.value = false;
  }
}

// Безпечне закриття overlay: модалка не закривається, якщо користувач
// почав drag всередині модалки і випадково відпустив кнопку поза нею.
const mouseDownOnOverlay = { value: false };
function onOverlayMouseDown(e: MouseEvent) {
  mouseDownOnOverlay.value = e.target === e.currentTarget;
}
function onOverlayClick(e: MouseEvent) {
  if (mouseDownOnOverlay.value && e.target === e.currentTarget) {
    emit('close');
  }
  mouseDownOnOverlay.value = false;
}
</script>

<template>
  <div class="modal-backdrop" @mousedown="onOverlayMouseDown" @click="onOverlayClick">
    <div class="modal-frame" @click.stop @mousedown.stop>
      <div class="modal" role="dialog" aria-modal="true" aria-labelledby="report-title">
        <button class="modal__close" type="button" :aria-label="STR.common.close" @click="$emit('close')">✕</button>

        <h2 id="report-title" class="modal__title">{{ t.title }}</h2>

        <label class="modal__field">
          <span class="modal__label">{{ t.reasonLabel }}</span>
          <select v-model="selectedReason" class="modal__select" :disabled="isSubmitting">
            <option value="">{{ t.chooseReason }}</option>
            <option v-for="r in reasons" :key="r" :value="r">{{ r }}</option>
          </select>
        </label>

        <p v-if="errorMessage" class="modal__error">{{ errorMessage }}</p>

        <button type="button" class="modal__submit" :disabled="isSubmitting || !selectedReason" @click="submit">
          {{ isSubmitting ? t.sending : STR.common.submit }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(49, 22, 32, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 1000;
}

.modal-frame {
  background: #3d0f1a;
  padding: 30px;
  border-radius: var(--radius-md);
  max-width: 440px;
  width: 100%;
  box-sizing: border-box;
}

.modal {
  position: relative;
  background: var(--color-modal-bg);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 24px 20px 20px;
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.modal__close {
  position: absolute;
  top: 8px;
  right: 12px;
  background: transparent;
  border: none;
  font-size: 20px;
  cursor: pointer;
  color: var(--text-on-light);
  padding: 4px 8px;
}

.modal__title {
  margin: 0;
  font-family: var(--font-display);
  font-size: 18px;
  font-weight: 400;
  text-align: center;
  color: var(--text-on-light);
}

.modal__field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.modal__label {
  font-family: var(--font-display);
  font-size: 14px;
  color: var(--text-on-light);
}

.modal__select {
  padding: 9px 12px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  font-family: var(--font-body);
  font-size: 14px;
  color: var(--text-on-light);
  cursor: pointer;
  outline: none;
}

.modal__select:focus {
  border-color: var(--color-primary);
}

.modal__error {
  margin: 0;
  font-family: var(--font-body);
  font-size: 13px;
  color: var(--text-error);
}

.modal__submit {
  align-self: stretch;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-md);
  padding: 10px 16px;
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  box-shadow: var(--shadow-sm);
  transition: background 0.15s;
}

.modal__submit:hover:not(:disabled) {
  background: var(--color-primary-hover);
}

.modal__submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
