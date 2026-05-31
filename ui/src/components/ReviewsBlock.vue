<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import type { ReviewResponse } from '../services/types';
import { fetchReviews, submitReview } from '../services/reviews';
import { useUserStore } from '../state/user';
import { useUserRatingsStore } from '../state/userRatings';
import { useNotificationsStore } from '../state/notifications';
import { extractErrorMessage } from '../services/error';
import ReviewItem from './ReviewItem.vue';
import ReportCommentModal from './ReportCommentModal.vue';

/**
 * SCRUM-72 (US 6.1) — блок коментарів на сторінці деталей твору.
 *  - Форма "Залиште коментар" + чекбокс "Спойлер" + кнопка "Надіслати".
 *  - Список існуючих відгуків (ReviewItem). Спойлер-блюр.
 *  - Кнопка "Поскаржитись" на кожному коментарі → ReportCommentModal.
 *  - Кнопка "Показати більше" — пагінація по 5 штук (інше — у наступному
 *    спринті, поки клієнтська пагінація достатньо).
 */

const props = defineProps<{
  workId: string;
}>();

const userStore = useUserStore();
const userRatings = useUserRatingsStore();
const notifications = useNotificationsStore();

const reviews = ref<ReviewResponse[]>([]);
const isLoading = ref(false);
const errorMessage = ref('');

// ── Форма коментаря ────────────────────────────────────────
const text = ref('');
const isSpoiler = ref(false);
const isSubmitting = ref(false);

// Скільки коментарів показуємо зараз (інші — за "Показати більше").
const PAGE_SIZE = 5;
const visibleCount = ref(PAGE_SIZE);

const visibleReviews = computed(() => reviews.value.slice(0, visibleCount.value));
const hasMore = computed(() => reviews.value.length > visibleCount.value);

// ── Модалка скарги ─────────────────────────────────────────
const reportingReviewId = ref<string | null>(null);

async function loadReviews(): Promise<void> {
  isLoading.value = true;
  errorMessage.value = '';
  try {
    reviews.value = await fetchReviews(props.workId);
    visibleCount.value = PAGE_SIZE;
  } catch (err) {
    errorMessage.value = extractErrorMessage(err);
    reviews.value = [];
  } finally {
    isLoading.value = false;
  }
}

async function onSubmit(): Promise<void> {
  if (!userStore.isAuthenticated) {
    notifications.pushWarning('Увійдіть, щоб написати відгук');
    return;
  }
  const trimmed = text.value.trim();
  if (trimmed.length < 10) {
    notifications.pushWarning('Відгук має бути не менше 10 символів');
    return;
  }
  isSubmitting.value = true;
  try {
    // Прив'язуємо рейтинг із userRatings store (середнє між book/film, якщо обидва є).
    const my = userRatings.getRating(props.workId, 'book');
    const myFilm = userRatings.getRating(props.workId, 'film');
    const rating = my && myFilm ? Math.round((my + myFilm) / 2) : my || myFilm || 0;

    const created = await submitReview({
      workId: props.workId,
      text: trimmed,
      isSpoiler: isSpoiler.value,
      rating,
      targetType: 'comparison',
    });
    // Додаємо у початок (найновіший зверху).
    reviews.value.unshift(created);
    text.value = '';
    isSpoiler.value = false;
    notifications.pushSuccess('Відгук опубліковано');
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  } finally {
    isSubmitting.value = false;
  }
}

function showMore(): void {
  visibleCount.value += PAGE_SIZE;
}

function onReport(reviewId: string): void {
  reportingReviewId.value = reviewId;
}

function closeReport(): void {
  reportingReviewId.value = null;
}

onMounted(loadReviews);
watch(
  () => props.workId,
  () => {
    visibleCount.value = PAGE_SIZE;
    loadReviews();
  }
);
</script>

<template>
  <section class="reviews">
    <h2 class="reviews__title">Коментарі</h2>

    <!-- ── Форма ────────────────────────────────────────── -->
    <form class="reviews__form" @submit.prevent="onSubmit">
      <div class="reviews__form-row">
        <div class="reviews__avatar" aria-hidden="true">
          {{ userStore.isAuthenticated ? (userStore.nickname || userStore.email || '?').charAt(0).toUpperCase() : '?' }}
        </div>
        <input
          v-model="text"
          type="text"
          class="reviews__input"
          placeholder="Залиште коментар"
          :disabled="isSubmitting"
          @keyup.enter="onSubmit"
        />
        <button
          type="submit"
          class="reviews__submit"
          :disabled="isSubmitting || !text.trim()"
        >
          {{ isSubmitting ? '…' : 'Надіслати' }}
        </button>
      </div>
      <label class="reviews__spoiler-toggle">
        <input v-model="isSpoiler" type="checkbox" :disabled="isSubmitting" />
        <span>Позначити як спойлер</span>
      </label>
    </form>

    <!-- ── Список ───────────────────────────────────────── -->
    <p v-if="isLoading" class="reviews__status">Завантаження…</p>

    <div v-else-if="errorMessage" class="reviews__status">
      <p>⚠ {{ errorMessage }}</p>
      <button type="button" class="reviews__retry" @click="loadReviews">Повторити</button>
    </div>

    <p v-else-if="reviews.length === 0" class="reviews__status">
      Будьте першим, хто залишить відгук
    </p>

    <ul v-else class="reviews__list">
      <li v-for="r in visibleReviews" :key="r.reviewId">
        <ReviewItem :review="r" @report="onReport" />
      </li>
    </ul>

    <button
      v-if="hasMore"
      type="button"
      class="reviews__more"
      @click="showMore"
    >
      Показати більше
    </button>

    <!-- ── Модалка скарги ──────────────────────────────── -->
    <ReportCommentModal
      v-if="reportingReviewId"
      :review-id="reportingReviewId"
      @close="closeReport"
    />
  </section>
</template>

<style scoped>
.reviews {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 24px 16px;
  margin: 0 16px;
  background: var(--color-page);
}

.reviews__title {
  margin: 0;
  font-family: var(--font-display);
  font-size: 20px;
  font-weight: 400;
  color: var(--text-on-light);
  text-align: center;
}

/* ── Форма ──────────────────────────────────────────────── */
.reviews__form {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.reviews__form-row {
  display: grid;
  grid-template-columns: 40px 1fr auto;
  gap: 10px;
  align-items: center;
}

.reviews__avatar {
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

.reviews__input {
  padding: 10px 14px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--color-input-bg);
  font-family: var(--font-body);
  font-size: 14px;
  color: var(--text-on-light);
  outline: none;
  width: 100%;
}

.reviews__input:focus {
  border-color: var(--color-primary);
}

.reviews__submit {
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-md);
  padding: 8px 16px;
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  white-space: nowrap;
}

.reviews__submit:hover:not(:disabled) {
  background: var(--color-primary-hover);
}

.reviews__submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.reviews__spoiler-toggle {
  align-self: flex-start;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-display);
  font-size: 13px;
  color: var(--text-on-light);
  cursor: pointer;
  margin-left: 50px;
}

.reviews__spoiler-toggle input {
  accent-color: var(--color-primary);
  cursor: pointer;
}

/* ── Список ─────────────────────────────────────────────── */
.reviews__list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.reviews__status {
  text-align: center;
  padding: 20px 0;
  color: var(--text-muted);
  font-family: var(--font-body);
  font-size: 14px;
}

.reviews__retry {
  margin-top: 8px;
  padding: 6px 16px;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 13px;
  font-family: var(--font-display);
}

.reviews__more {
  align-self: center;
  background: var(--color-card);
  color: var(--text-on-dark);
  border: 1px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 8px 32px;
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  margin-top: 8px;
}

.reviews__more:hover {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

@media (max-width: 600px) {
  .reviews__form-row {
    grid-template-columns: 40px 1fr;
  }
  .reviews__submit {
    grid-column: 2;
    width: 100%;
  }
}
</style>
