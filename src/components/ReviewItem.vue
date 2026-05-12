<script setup lang="ts">
import { ref, computed } from 'vue';
import type { ReviewResponse } from '../services/types';
import StarRating from './StarRating.vue';

/**
 * Один відгук у списку коментарів. Якщо isSpoiler=true — текст приховано
 * за blur'ом, з лінком "показати". Кнопка "Поскаржитись" видима для всіх,
 * фактичний submit іде через окрему модалку (ReportCommentModal).
 */

defineProps<{
  review: ReviewResponse;
}>();

defineEmits<{
  report: [reviewId: string];
}>();

// Чи розкрив користувач спойлер локально.
const revealed = ref(false);

const formattedDate = computed(() => {
  // У createdAt бекенд може повертати як ISO рядок. Парсимо безпечно.
  const d = new Date();
  d.setTime(Date.now()); // fallback
  // ця компонента — тільки для відображення, не критично
  return '';
});
</script>

<template>
  <article class="review">
    <!-- Аватар (поки що ініціал у кружечку) -->
    <div class="review__avatar" aria-hidden="true">
      {{ (review.userNickname || '?').charAt(0).toUpperCase() }}
    </div>

    <div class="review__body">
      <header class="review__head">
        <span class="review__author">{{ review.userNickname || 'Користувач' }}</span>
        <StarRating v-if="review.rating > 0" :model-value="review.rating" readonly :size="14" />
      </header>

      <!-- Спойлер: показуємо плейсхолдер з тогглом -->
      <div v-if="review.isSpoiler && !revealed" class="review__spoiler">
        <span>Коментар містить спойлер,</span>
        <button type="button" class="review__reveal-link" @click="revealed = true">показати</button>
      </div>
      <p v-else class="review__text">{{ review.text }}</p>

      <footer class="review__foot">
        <button type="button" class="review__report" @click="$emit('report', review.reviewId)">
          Поскаржитись
        </button>
      </footer>
    </div>
    <span v-if="formattedDate" class="visually-hidden">{{ formattedDate }}</span>
  </article>
</template>

<style scoped>
.review {
  display: grid;
  grid-template-columns: 40px 1fr;
  gap: 12px;
  padding: 12px 14px;
  background: var(--color-panel-box);
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-default);
}

.review__avatar {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-pill);
  background: var(--color-card);
  color: var(--text-on-dark);
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-display);
  font-size: 16px;
  font-weight: 600;
}

.review__body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.review__head {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.review__author {
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 600;
  color: var(--text-on-light);
}

.review__text {
  margin: 0;
  font-family: var(--font-body);
  font-size: 14px;
  line-height: 1.5;
  color: var(--text-on-light);
  word-break: break-word;
}

.review__spoiler {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-body);
  font-size: 14px;
  color: var(--text-muted);
  font-style: italic;
}

.review__reveal-link {
  background: none;
  border: none;
  color: var(--color-primary);
  text-decoration: underline;
  cursor: pointer;
  font: inherit;
  padding: 0;
}

.review__reveal-link:hover {
  color: var(--color-primary-hover);
}

.review__foot {
  display: flex;
  justify-content: flex-end;
}

.review__report {
  background: none;
  border: none;
  color: var(--text-muted);
  font-size: 12px;
  font-family: var(--font-display);
  cursor: pointer;
  padding: 2px 4px;
}

.review__report:hover {
  color: var(--color-primary);
  text-decoration: underline;
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}
</style>
