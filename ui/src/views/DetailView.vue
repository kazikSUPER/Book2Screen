<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { BookScreenItem, DifferencePoint } from '../services/types';
import { fetchWorkById } from '../services/works';
import { extractErrorMessage } from '../services/error';
import { useWishlistStore } from '../state/wishlist';
import { useUserRatingsStore } from '../state/userRatings';
import { useUserStore } from '../state/user';
import { useNotificationsStore } from '../state/notifications';
import StarRating from '../components/StarRating.vue';
import DifferencesMap from '../components/DifferencesMap.vue';
import VotingBlock from '../components/VotingBlock.vue';
import ReviewsBlock from '../components/ReviewsBlock.vue';
import { STR } from '../constants';

const t = STR.detail;

/**
 * SCRUM-68 (US 3.2) — Book Details.
 *
 * Сторінка деталей твору, повна за дизайном Figma:
 *  - брейкрумна стрічка з категорією/назвою
 *  - дві картки порівняння (книга / екранізація) з постерами, метою, summary
 *  - кнопки "+ Хочу прочитати / переглянути" (через wishlist store)
 *  - зіркові оцінки (інтерактивні — userRatings store)
 *  - інтерактивна карта відмінностей (компонент DifferencesMap)
 *  - блок голосування "Книга vs Фільм" (VotingBlock, SCRUM-70)
 *  - блок коментарів/відгуків (ReviewsBlock, SCRUM-72)
 */

const route = useRoute();
const router = useRouter();

const item = ref<BookScreenItem | null>(null);
const isLoading = ref(false);
const errorMessage = ref('');

const wishlist = useWishlistStore();
const userRatings = useUserRatingsStore();
const userStore = useUserStore();
const notifications = useNotificationsStore();

async function loadItem(): Promise<void> {
  isLoading.value = true;
  errorMessage.value = '';
  try {
    item.value = await fetchWorkById(route.params.id as string);
  } catch (err) {
    errorMessage.value = extractErrorMessage(err);
    item.value = null;
  } finally {
    isLoading.value = false;
  }
}

onMounted(loadItem);
watch(() => route.params.id, loadItem);

// ── Wishlist (хочу прочитати / переглянути) ─────────────────
const inReadList = computed(() => (item.value ? wishlist.isInWishlist(item.value.id, 'read') : false));
const inWatchList = computed(() => (item.value ? wishlist.isInWishlist(item.value.id, 'watch') : false));

function toggleRead(): void {
  if (!item.value) return;
  if (!userStore.isAuthenticated) {
    notifications.pushWarning(t.wishlistNeedAuth);
    return;
  }
  wishlist.toggle(item.value.id, 'read');
}

function toggleWatch(): void {
  if (!item.value) return;
  if (!userStore.isAuthenticated) {
    notifications.pushWarning(t.wishlistNeedAuth);
    return;
  }
  wishlist.toggle(item.value.id, 'watch');
}

// ── User-ratings (зіркова оцінка) ───────────────────────────
const myBookRating = computed({
  get: () => (item.value ? userRatings.getRating(item.value.id, 'book') : 0),
  set: (v: number) => {
    if (!item.value) return;
    if (!userStore.isAuthenticated) {
      notifications.pushWarning(t.ratingNeedAuth);
      return;
    }
    userRatings.setRating(item.value.id, 'book', v);
  },
});

const myFilmRating = computed({
  get: () => (item.value ? userRatings.getRating(item.value.id, 'film') : 0),
  set: (v: number) => {
    if (!item.value) return;
    if (!userStore.isAuthenticated) {
      notifications.pushWarning(t.ratingNeedAuth);
      return;
    }
    userRatings.setRating(item.value.id, 'film', v);
  },
});

// ── Помічники для двокарткового layout ──────────────────────
const filmYearLabel = computed(() => item.value?.filmYear ?? item.value?.year ?? '');
const filmCountryLabel = computed(() => item.value?.filmCountry ?? item.value?.country ?? '');
const bookSummary = computed(() => item.value?.bookSummary ?? item.value?.description ?? '');
const filmSummary = computed(() => item.value?.filmSummary ?? item.value?.description ?? '');

watch(
  item,
  (newItem) => {
    if (newItem) {
      document.title = `${newItem.title} — порівняння книги та екранізації — Book2Screen`;
    }
  },
  { immediate: true }
);

const goBack = () => router.push({ name: 'home' });

const defaultDifferences: DifferencePoint[] = [];

const differencesData = computed(() => {
  if (item.value?.differences && item.value.differences.length > 0) {
    return item.value.differences;
  }
  return defaultDifferences;
});
</script>

<template>
  <div class="detail">
    <!-- Брейкрумна стрічка під шапкою (з Figma) -->
    <div v-if="item" class="detail__crumb">
      <button class="detail__crumb-back" type="button" :aria-label="STR.common.back" @click="goBack">←</button>
      <span class="detail__crumb-text">{{ item.genre }} / {{ item.title }}</span>
    </div>

    <p v-if="isLoading" class="detail__status">{{ STR.common.loading }}</p>

    <div v-else-if="errorMessage" class="detail__status">
      <p>⚠ {{ errorMessage }}</p>
      <button class="detail__retry" type="button" @click="loadItem">{{ STR.common.retry }}</button>
    </div>

    <p v-else-if="!item" class="detail__status">{{ t.notFound }}</p>

    <template v-else>
      <h1 class="sr-only">Порівняння твору: {{ item.title }} (книга та екранізація)</h1>
      <!-- ── Дві картки порівняння ─────────────────────────── -->
      <!-- ── Дві картки порівняння ─────────────────────────── -->
      <section class="detail__compare">
        <!-- Книга -->
        <div class="compare-col">
          <article class="compare-card">
            <div class="compare-card__main">
              <div class="compare-card__poster">
                <img :src="item.poster" :alt="item.title" loading="lazy" />
              </div>
              <div class="compare-card__info">
                <h2 class="compare-card__title">{{ item.title }}</h2>
                <dl class="compare-card__meta">
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookYear }}</dt>
                    <dd>{{ item.bookYear ?? item.year }}</dd>
                  </div>
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookGenre }}</dt>
                    <dd>{{ item.genre }}</dd>
                  </div>
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookCountry }}</dt>
                    <dd>{{ item.country }}</dd>
                  </div>
                  <div v-if="item.author" class="compare-card__meta-row">
                    <dt>{{ t.bookAuthor }}</dt>
                    <dd>{{ item.author }}</dd>
                  </div>
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookRating }}</dt>
                    <dd>{{ item.bookRating }} / 5</dd>
                  </div>
                </dl>
                <button
                  class="compare-card__wish"
                  :class="{ 'compare-card__wish--active': inReadList }"
                  type="button"
                  @click="toggleRead"
                >
                  <span class="compare-card__wish-icon">{{ inReadList ? '✓' : '+' }}</span>
                  {{ inReadList ? t.inList : t.wantToRead }}
                </button>
              </div>
            </div>
            <div class="compare-card__summary">
              <div class="compare-card__summary-title">{{ t.summaryTitle }}</div>
              <p class="compare-card__summary-text">{{ bookSummary }}</p>
            </div>
          </article>
          <div class="user-rating">
            <span class="user-rating__label">{{ t.bookRatingLabel }}</span>
            <StarRating v-model="myBookRating" :size="32" />
          </div>
        </div>

        <!-- VS / Екранізовано -->
        <div class="detail__vs" aria-hidden="true">
          <span class="detail__vs-text">{{ t.adapted }}</span>
          <svg width="60" height="20" viewBox="0 0 60 20" fill="none">
            <path
              d="M0 10H55M55 10L46 2M55 10L46 18"
              stroke="currentColor"
              stroke-width="1.5"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        </div>

        <!-- Екранізація -->
        <div class="compare-col">
          <article class="compare-card compare-card--mirror">
            <div class="compare-card__main">
              <div class="compare-card__info">
                <h2 class="compare-card__title">{{ item.title }}</h2>
                <dl class="compare-card__meta">
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookYear }}</dt>
                    <dd>{{ filmYearLabel }}</dd>
                  </div>
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookGenre }}</dt>
                    <dd>{{ item.genre }}</dd>
                  </div>
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookCountry }}</dt>
                    <dd>{{ filmCountryLabel }}</dd>
                  </div>
                  <div v-if="item.director" class="compare-card__meta-row">
                    <dt>{{ t.bookDirector }}</dt>
                    <dd>{{ item.director }}</dd>
                  </div>
                  <div class="compare-card__meta-row">
                    <dt>{{ t.bookRating }}</dt>
                    <dd>{{ item.filmRating }} / 5</dd>
                  </div>
                </dl>
                <button
                  class="compare-card__wish"
                  :class="{ 'compare-card__wish--active': inWatchList }"
                  type="button"
                  @click="toggleWatch"
                >
                  <span class="compare-card__wish-icon">{{ inWatchList ? '✓' : '+' }}</span>
                  {{ inWatchList ? t.inList : t.wantToWatch }}
                </button>
              </div>
              <div class="compare-card__poster">
                <img :src="item.filmPoster ?? item.poster" :alt="item.title" loading="lazy" />
              </div>
            </div>
            <div class="compare-card__summary">
              <div class="compare-card__summary-title">{{ t.summaryTitle }}</div>
              <p class="compare-card__summary-text">{{ filmSummary }}</p>
            </div>
          </article>
          <div class="user-rating">
            <span class="user-rating__label">{{ t.filmRatingLabel }}</span>
            <StarRating v-model="myFilmRating" :size="32" />
          </div>
        </div>
      </section>

      <!-- ── Карта відмінностей (інтерактивна) ─────────────── -->
      <DifferencesMap :points="differencesData" />

      <!-- ── Голосування (SCRUM-70 / SCRUM-71) ─────────────── -->
      <VotingBlock :work-id="item.id" />

      <!-- ── Коментарі (SCRUM-72) ──────────────────────────── -->
      <ReviewsBlock :work-id="item.id" />
    </template>
  </div>
</template>

<style scoped>
.detail {
  display: flex;
  flex-direction: column;
  gap: 24px;
  font-family: var(--font-body);
  color: var(--text-on-light);
  padding-bottom: 32px;
}

/* ── Брейкрумна стрічка ─────────────────────────────────── */
.detail__crumb {
  background: var(--color-card);
  color: var(--text-on-dark);
  padding: 10px 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  font-family: var(--font-display);
  font-size: 14px;
  margin: -16px -16px 0 -16px;
}

.detail__crumb-back {
  background: transparent;
  border: none;
  color: inherit;
  font-size: 20px;
  cursor: pointer;
  padding: 8px 16px;
  line-height: 1;
  min-width: 44px;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.detail__crumb-back:hover {
  color: var(--color-panel-box);
}

/* ── Двокартковий блок ──────────────────────────────────── */
.detail__compare {
  display: flex;
  justify-content: center;
  gap: 20px;
  padding: 16px;
  align-items: center;
  flex-wrap: wrap;
}

.compare-col {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 0 0 300px;
  max-width: 450px;
  flex: 1;
  min-width: 320px;
}

.compare-card {
  background-color: #391418; /* Темно-бордовий фон */
  border: 1px solid #23080a;
  border-radius: var(--radius-sm);
  padding: 16px;
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: 16px;
  color: var(--text-on-dark);
  height: 370px; /* Фіксована висота, щоб блоки не розтягувались */
}

.compare-card__main {
  display: flex;
  gap: 16px;
  align-items: flex-start;
}

.compare-card__info {
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
}

.compare-card__title {
  margin: 0;
  font-family: var(--font-body);
  font-size: 16px;
  font-weight: 500;
  text-align: center;
}

.compare-card__poster {
  width: 130px;
  height: 190px;
  flex-shrink: 0;
  overflow: hidden;
  border-radius: var(--radius-xs);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
}

.compare-card__poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.compare-card__meta {
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 13px;
  font-family: var(--font-body);
}

.compare-card__meta-row {
  display: flex;
  gap: 6px;
}

.compare-card__meta-row dt {
  font-weight: 500;
}

.compare-card__meta-row dd {
  margin: 0;
}

.compare-card__wish {
  background-color: #721c24; /* Вишневий фон кнопки */
  border: 1px solid #5a141c;
  color: #fff;
  border-radius: var(--radius-sm);
  padding: 8px 16px;
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.15s;
  align-self: center; /* По центру колонки інфо */
  width: 100%;
}

.compare-card__wish:hover {
  background-color: #8c232c;
}

.compare-card__wish--active {
  background-color: #5a141c;
}

.compare-card__wish-icon {
  font-size: 18px;
  line-height: 1;
  font-weight: bold;
}

.compare-card__summary {
  background-color: #872832; /* Світло-вишневий фон анотації */
  border-radius: var(--radius-sm);
  padding: 12px;
  text-align: center;
  flex: 1; /* Займати весь залишок місця в картці */
  display: flex;
  flex-direction: column;
  overflow: hidden; /* Обрізати, якщо виходить за межі */
}

.compare-card__summary-title {
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 6px;
  flex-shrink: 0;
}

.compare-card__summary-text {
  margin: 0;
  font-size: 13px;
  line-height: 1.4;
  font-family: var(--font-body);
  flex: 1;
  overflow-y: auto; /* Скрол для довгого тексту */
  padding-right: 4px; /* Відступ для скролбару */
}

/* Стилізація скролбару для анотації */
.compare-card__summary-text::-webkit-scrollbar {
  width: 4px;
}
.compare-card__summary-text::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.1);
  border-radius: 4px;
}
.compare-card__summary-text::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 4px;
}
.compare-card__summary-text::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}

/* ── VS блок між картками ───────────────────────────────── */
.detail__vs {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  color: var(--text-on-light);
  font-family: var(--font-display);
  font-size: 15px;
}

.detail__vs-text {
  white-space: nowrap;
  font-weight: 600;
  color: #333; /* Темний колір для стрілки та тексту */
}

.detail__vs svg {
  color: #333;
}

/* ── Зіркові оцінки під кожною карткою ───────────────────── */
.user-rating {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.user-rating__label {
  font-family: var(--font-display);
  font-size: 15px;
  color: #333;
  font-weight: 500;
}

/* ── Status ─────────────────────────────────────────────── */
.detail__status {
  text-align: center;
  padding: 60px 0;
  color: var(--text-muted);
  font-size: 16px;
}

.detail__retry {
  margin-top: 12px;
  padding: 8px 20px;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 14px;
  font-family: var(--font-display);
}

.detail__retry:hover {
  background: var(--color-primary-hover);
}

/* ── Адаптив ────────────────────────────────────────────── */
@media (max-width: 900px) {
  .detail__compare {
    grid-template-columns: 1fr;
  }
  .detail__vs {
    flex-direction: row;
  }
  .detail__vs svg {
    transform: rotate(90deg);
  }
}

@media (max-width: 600px) {
  .compare-card__row {
    grid-template-columns: 1fr;
    justify-items: center;
  }
}
</style>
