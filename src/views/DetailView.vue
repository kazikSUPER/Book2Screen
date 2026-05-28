<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { BookScreenItem } from '../services/types';
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
 *  - блоки голосування і коментарів — заглушки під SCRUM-70/71/72
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

const goBack = () => router.push({ name: 'home' });
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
      <!-- ── Дві картки порівняння ─────────────────────────── -->
      <section class="detail__compare">
        <!-- Книга -->
        <article class="compare-card">
          <h2 class="compare-card__title">{{ item.title }}</h2>
          <div class="compare-card__row">
            <div class="compare-card__poster">
              <img :src="item.poster" :alt="item.title" loading="lazy" />
            </div>
            <dl class="compare-card__meta">
              <div class="compare-card__meta-row">
                <dt>{{ t.bookYear }}</dt>
                <dd>{{ item.year }}</dd>
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
                <dd>{{ item.bookRating }} / 10</dd>
              </div>
            </dl>
          </div>
          <button
            class="compare-card__wish"
            :class="{ 'compare-card__wish--active': inReadList }"
            type="button"
            @click="toggleRead"
          >
            <span class="compare-card__wish-icon">{{ inReadList ? '✓' : '+' }}</span>
            {{ inReadList ? t.inList : t.wantToRead }}
          </button>
          <p class="compare-card__summary">
            <strong>{{ t.summaryTitle }}</strong
            ><br />
            {{ bookSummary }}
          </p>
        </article>

        <!-- VS / Екранізовано -->
        <div class="detail__vs" aria-hidden="true">
          <span class="detail__vs-text">{{ t.adapted }}</span>
          <svg width="40" height="20" viewBox="0 0 40 20" fill="none">
            <path d="M0 10H35M35 10L26 2M35 10L26 18" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
        </div>

        <!-- Екранізація (за Figma — дзеркальна: текст ліворуч, постер праворуч) -->
        <article class="compare-card compare-card--mirror">
          <h2 class="compare-card__title">{{ item.title }}</h2>
          <div class="compare-card__row">
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
                <dd>{{ item.filmRating }} / 10</dd>
              </div>
            </dl>
            <div class="compare-card__poster">
              <img :src="item.filmPoster ?? item.poster" :alt="item.title" loading="lazy" />
            </div>
          </div>
          <button
            class="compare-card__wish"
            :class="{ 'compare-card__wish--active': inWatchList }"
            type="button"
            @click="toggleWatch"
          >
            <span class="compare-card__wish-icon">{{ inWatchList ? '✓' : '+' }}</span>
            {{ inWatchList ? t.inList : t.wantToWatch }}
          </button>
          <p class="compare-card__summary">
            <strong>{{ t.summaryTitle }}</strong
            ><br />
            {{ filmSummary }}
          </p>
        </article>
      </section>

      <!-- ── Зіркові оцінки користувача ─────────────────────── -->
      <section class="detail__user-ratings">
        <div class="user-rating">
          <span class="user-rating__label">{{ t.bookRatingLabel }}</span>
          <StarRating v-model="myBookRating" :size="32" />
        </div>
        <div class="user-rating">
          <span class="user-rating__label">{{ t.filmRatingLabel }}</span>
          <StarRating v-model="myFilmRating" :size="32" />
        </div>
      </section>

      <!-- ── Карта відмінностей (інтерактивна) ─────────────── -->
      <DifferencesMap v-if="item.differences && item.differences.length" :points="item.differences" />

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
  padding: 4px 8px;
  line-height: 1;
}

.detail__crumb-back:hover {
  color: var(--color-panel-box);
}

/* ── Двокартковий блок ──────────────────────────────────── */
.detail__compare {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  gap: 16px;
  align-items: stretch;
  padding: 16px;
}

.compare-card {
  background: var(--color-card);
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  padding: 16px;
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: 12px;
  color: var(--text-on-dark);
}

.compare-card__title {
  margin: 0;
  font-family: var(--font-body);
  font-size: 16px;
  text-align: center;
  font-weight: 500;
}

.compare-card__row {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 12px;
  align-items: start;
}

/* Дзеркальна картка екранізації (за Figma — постер праворуч, текст ліворуч). */
.compare-card--mirror .compare-card__row {
  grid-template-columns: 1fr auto;
}

.compare-card__poster {
  width: 110px;
  height: 160px;
  flex-shrink: 0;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xs);
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
  align-self: flex-start;
  background: transparent;
  border: 1px solid var(--text-on-dark);
  color: var(--text-on-dark);
  border-radius: var(--radius-sm);
  padding: 6px 14px;
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.15s;
}

.compare-card__wish:hover {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.compare-card__wish--active {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.compare-card__wish-icon {
  font-size: 16px;
  line-height: 1;
}

.compare-card__summary {
  margin: 0;
  font-size: 13px;
  line-height: 1.5;
  color: var(--text-on-dark);
}

.compare-card__summary strong {
  font-family: var(--font-display);
  font-size: 13px;
  font-weight: 500;
}

/* ── VS блок між картками ───────────────────────────────── */
.detail__vs {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: var(--text-on-light);
  font-family: var(--font-display);
  font-size: 14px;
}

.detail__vs-text {
  white-space: nowrap;
  font-weight: 500;
}

/* ── Зіркові оцінки ─────────────────────────────────────── */
.detail__user-ratings {
  display: flex;
  justify-content: space-around;
  gap: 16px;
  padding: 0 16px;
  flex-wrap: wrap;
}

.user-rating {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.user-rating__label {
  font-family: var(--font-display);
  font-size: 14px;
  color: var(--text-on-light);
}

/* ── Заглушки голосування + коментарі ───────────────────── */
.detail__placeholder {
  background: var(--color-panel-box);
  border-radius: var(--radius-md);
  padding: 24px;
  margin: 0 16px;
  text-align: center;
  border: 1px dashed var(--color-card);
}

.detail__placeholder-title {
  margin: 0 0 8px 0;
  font-family: var(--font-display);
  font-size: 18px;
  color: var(--text-on-light);
}

.detail__placeholder-note {
  margin: 0;
  font-size: 13px;
  color: var(--text-muted);
  font-family: var(--font-body);
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
