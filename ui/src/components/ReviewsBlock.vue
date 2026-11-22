<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import type { ReviewResponse, ReviewTargetType } from '../services/types';
import { fetchReviews, submitReview } from '../services/reviews';
import { useUserStore } from '../state/user';
import { useUserRatingsStore } from '../state/userRatings';
import { useNotificationsStore } from '../state/notifications';
import { extractErrorMessage } from '../services/error';
import ReviewItem from './ReviewItem.vue';
import ReportCommentModal from './ReportCommentModal.vue';
import { STR } from '../constants';

const t = STR.detail;

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
const targetType = ref<ReviewTargetType>('comparison');
const PAGE_SIZE = 5;
const visibleCount = ref(PAGE_SIZE);

const visibleReviews = computed(() => reviews.value.slice(0, visibleCount.value));
const hasMore = computed(() => reviews.value.length > visibleCount.value);

// BUG-037: live-індикатор перевищення довжини.
const isTooLong = computed(() => text.value.length > 2000);

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
    notifications.pushWarning(t.commentNeedAuth);
    return;
  }
  const trimmed = text.value.trim();
  // BUG-037 / BUG-030: бек чекає 10..2000 символів. Валідуємо клієнтсько.
  if (trimmed.length < 10) {
    notifications.pushWarning(t.commentTooShort);
    return;
  }
  if (trimmed.length > 2000) {
    notifications.pushWarning(t.commentTooLong);
    return;
  }
  isSubmitting.value = true;
  try {
    // Прив'язуємо рейтинг із userRatings (середнє між book/film, якщо обидва є).
    // У store рейтинг 1..5, на бекенді ReviewRequest очікує 0..10 — конвертуємо ×2.
    const my = userRatings.getRating(props.workId, 'book');
    const myFilm = userRatings.getRating(props.workId, 'film');
    const stars = my && myFilm ? (my + myFilm) / 2 : my || myFilm || 0;
    const rating = Math.round(stars * 2); // 1..5 → 2..10

    const created = await submitReview({
      workId: props.workId,
      text: trimmed,
      isSpoiler: isSpoiler.value,
      rating,
      targetType: targetType.value,
    });
    // Додаємо у початок (найновіший зверху).
    reviews.value.unshift(created);
    text.value = '';
    isSpoiler.value = false;
    targetType.value = 'comparison';
    notifications.pushSuccess(t.commentPublished);
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
    <h2 class="reviews__title">{{ t.commentsTitle }}</h2>

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
          :class="{ 'reviews__input--error': isTooLong }"
          :placeholder="t.commentPlaceholder"
          :maxlength="2200"
          :disabled="isSubmitting"
        />
        <button type="submit" class="reviews__submit" :disabled="isSubmitting || !text.trim() || isTooLong">
          {{ isSubmitting ? '…' : STR.common.submit }}
        </button>
      </div>
      <div class="reviews__row-meta">
        <label class="reviews__spoiler-toggle">
          <input v-model="isSpoiler" type="checkbox" :disabled="isSubmitting" />
          <span>{{ t.markSpoiler }}</span>
        </label>

        <select v-model="targetType" :disabled="isSubmitting" class="reviews__select">
          <option value="comparison">{{ t.targetComparison }}</option>
          <option value="book">{{ t.targetBook }}</option>
          <option value="adaptation">{{ t.targetAdaptation }}</option>
        </select>

        <!-- BUG-037: лічильник символів. Червоніє при перевищенні. -->
        <span class="reviews__counter" :class="{ 'reviews__counter--error': isTooLong }">
          {{ text.length }} / 2000
        </span>
      </div>
    </form>

    <!-- ── Список ───────────────────────────────────────── -->
    <p v-if="isLoading" class="reviews__status">{{ STR.common.loading }}</p>

    <div v-else-if="errorMessage" class="reviews__status">
      <p>⚠ {{ errorMessage }}</p>
      <button type="button" class="reviews__retry" @click="loadReviews">{{ STR.common.retry }}</button>
    </div>

    <p v-else-if="reviews.length === 0" class="reviews__status">
      {{ t.commentEmptyList }}
    </p>

    <ul v-else class="reviews__list">
      <li v-for="r in visibleReviews" :key="r.reviewId">
        <ReviewItem :review="r" @report="onReport" />
      </li>
    </ul>

    <button v-if="hasMore" type="button" class="reviews__more" @click="showMore">
      {{ STR.common.showMore }}
    </button>

    <!-- ── Модалка скарги ──────────────────────────────── -->
    <ReportCommentModal v-if="reportingReviewId" :review-id="reportingReviewId" @close="closeReport" />
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

/* BUG-037: рядок під полем — чекбокс зліва + лічильник справа. */
.reviews__row-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-left: 50px;
}

.reviews__row-meta .reviews__spoiler-toggle {
  margin-left: 0;
}

.reviews__counter {
  font-family: var(--font-display);
  font-size: 12px;
  color: var(--text-muted);
}

.reviews__counter--error {
  color: var(--text-error);
  font-weight: 600;
}

.reviews__input--error {
  border-color: var(--text-error) !important;
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

.reviews__select {
  padding: 4px 8px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  color: var(--text-on-light);
  font-family: inherit;
  font-size: 12px;
  outline: none;
  cursor: pointer;
}

.reviews__select:focus {
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
