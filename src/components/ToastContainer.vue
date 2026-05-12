<script setup lang="ts">
import { useNotificationsStore } from '../state/notifications';

const notifications = useNotificationsStore();
</script>

<template>
  <div class="toast-container" aria-live="polite">
    <transition-group name="toast">
      <div
        v-for="n in notifications.items"
        :key="n.id"
        class="toast"
        :class="`toast-${n.type}`"
        role="status"
        @click="notifications.remove(n.id)"
      >
        <span class="toast-icon" aria-hidden="true">
          {{ n.type === 'success' ? '✓' : n.type === 'error' ? '✕' : n.type === 'warning' ? '!' : 'ℹ' }}
        </span>
        <span class="toast-message">{{ n.message }}</span>
        <button class="toast-close" :aria-label="'Закрити'" @click.stop="notifications.remove(n.id)">×</button>
      </div>
    </transition-group>
  </div>
</template>

<style scoped>
.toast-container {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 1000;
  display: flex;
  flex-direction: column;
  gap: 10px;
  pointer-events: none;
  max-width: calc(100vw - 32px);
}

.toast {
  pointer-events: auto;
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 280px;
  max-width: 400px;
  padding: 12px 14px;
  border-radius: var(--radius-md);
  font-family: var(--font-body);
  font-size: 14px;
  line-height: 1.4;
  box-shadow: var(--shadow-lg);
  cursor: pointer;
  color: var(--text-on-primary);
}

.toast-success {
  background-color: #2e7d32;
  border-left: 4px solid #1b5e20;
}

.toast-error {
  background-color: var(--text-error);
  border-left: 4px solid var(--color-primary);
}

.toast-warning {
  background-color: #ef6c00;
  border-left: 4px solid #b34700;
}

.toast-info {
  background-color: var(--color-header);
  border-left: 4px solid var(--color-primary);
}

.toast-icon {
  font-size: 18px;
  font-weight: 700;
  flex-shrink: 0;
}

.toast-message {
  flex: 1;
  word-break: break-word;
}

.toast-close {
  background: transparent;
  border: none;
  color: inherit;
  font-size: 20px;
  line-height: 1;
  cursor: pointer;
  padding: 0 4px;
  opacity: 0.7;
  flex-shrink: 0;
}

.toast-close:hover {
  opacity: 1;
}

/* Анімація появи/зникнення */
.toast-enter-active,
.toast-leave-active {
  transition:
    transform 0.25s ease,
    opacity 0.25s ease;
}

.toast-enter-from {
  transform: translateX(20px);
  opacity: 0;
}

.toast-leave-to {
  transform: translateX(20px);
  opacity: 0;
}

/* Мобільна адаптація — тости від краю до краю */
@media (max-width: 480px) {
  .toast-container {
    top: auto;
    bottom: 16px;
    right: 16px;
    left: 16px;
    max-width: none;
  }

  .toast {
    min-width: 0;
    max-width: none;
  }
}
</style>
