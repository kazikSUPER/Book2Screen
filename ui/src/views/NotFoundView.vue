<script setup lang="ts">
import { useRouter } from 'vue-router';

/**
 * BUG-043 — Кастомна 404 сторінка.
 * Замість редіректу на головну показуємо повідомлення і кнопку повернення.
 */

const router = useRouter();

function goHome(): void {
  router.push({ name: 'home' });
}
</script>

<template>
  <div class="not-found">
    <div class="not-found__icon" aria-hidden="true">🎬📕</div>
    <h1 class="not-found__title">404</h1>
    <p class="not-found__text">Сторінку не знайдено</p>
    <p class="not-found__sub">Можливо, ви перейшли за застарілим лінком або помилково ввели адресу.</p>
    <button type="button" class="not-found__btn" @click="goHome">На головну</button>
  </div>
</template>

<style scoped>
.not-found {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  text-align: center;
  gap: 12px;
  min-height: 60vh;
  font-family: var(--font-body);
}

.not-found__icon {
  font-size: 64px;
  line-height: 1;
}

.not-found__title {
  margin: 0;
  font-family: var(--font-display);
  font-size: 96px;
  font-weight: 400;
  color: var(--color-card);
  line-height: 1;
}

.not-found__text {
  margin: 0;
  font-family: var(--font-display);
  font-size: 24px;
  color: var(--text-on-light);
}

.not-found__sub {
  margin: 0 0 12px;
  font-size: 14px;
  color: var(--text-muted);
  max-width: 480px;
}

.not-found__btn {
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-md);
  padding: 10px 32px;
  font-family: var(--font-display);
  font-size: 16px;
  cursor: pointer;
  box-shadow: var(--shadow-md);
  transition: background 0.2s;
}

.not-found__btn:hover {
  background: var(--color-primary-hover);
}

@media (max-width: 600px) {
  .not-found__title {
    font-size: 64px;
  }
  .not-found__text {
    font-size: 18px;
  }
}
</style>
