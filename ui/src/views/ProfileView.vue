<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../state/user';
import { useUserRatingsStore } from '../state/userRatings';
import { useWishlistStore } from '../state/wishlist';
import { useNotificationsStore } from '../state/notifications';
import { fetchWorks } from '../services/works';
import { fetchMyReviews, deleteMyReview } from '../services/profile';
import { extractErrorMessage } from '../services/error';
import type { BookScreenItem, ReviewResponse } from '../services/types';
import StarRating from '../components/StarRating.vue';
import WorkCard from '../components/WorkCard.vue';
import { STR } from '../constants';
import { onImgError } from '../composables/useImageFallback';

const t = STR.profile;

/**
 * SCRUM-64 (US 1.3) — Personal profile.
 *
 * Сторінка профілю з 4-ма секціями за дизайном Figma:
 *   1. Шапка: аватар + "Змінити фото" + інфо-картка + статистика.
 *   2. "Мої оцінки" — твори, яким користувач поставив зіркову оцінку.
 *   3. "Мої відгуки" — список з кнопками редагувати/видалити (mock).
 *   4. "Хочу переглянути/прочитати" — твори з wishlist'у.
 *
 * Якщо неавторизований — редірект на головну (route guard у onMounted).
 */

const router = useRouter();
const userStore = useUserStore();
const userRatings = useUserRatingsStore();
const wishlist = useWishlistStore();
const notifications = useNotificationsStore();

const allWorks = ref<BookScreenItem[]>([]);
const myReviews = ref<ReviewResponse[]>([]);
const isLoading = ref(false);

// "Редагувати" профіль у режимі inline.
const editing = ref(false);
const editForm = ref({
  username: '',
  nickname: '',
});

// "Показати більше" для відгуків.
const REVIEWS_PAGE = 2;
const visibleReviewsCount = ref(REVIEWS_PAGE);

const visibleReviews = computed(() => myReviews.value.slice(0, visibleReviewsCount.value));
const hasMoreReviews = computed(() => myReviews.value.length > visibleReviewsCount.value);

// ── Списки на основі сторів ─────────────────────────────────
const ratedWorks = computed<Array<{ work: BookScreenItem; bookRating: number; filmRating: number }>>(() => {
  return userRatings.ratedWorkIds
    .map((id) => allWorks.value.find((w) => w.id === id))
    .filter((w): w is BookScreenItem => Boolean(w))
    .map((w) => ({
      work: w,
      bookRating: userRatings.getRating(w.id, 'book'),
      filmRating: userRatings.getRating(w.id, 'film'),
    }));
});

const readWorks = computed<BookScreenItem[]>(() => {
  return wishlist.readList
    .map((id) => allWorks.value.find((w) => w.id === id))
    .filter((w): w is BookScreenItem => Boolean(w));
});

const watchWorks = computed<BookScreenItem[]>(() => {
  return wishlist.watchList
    .map((id) => allWorks.value.find((w) => w.id === id))
    .filter((w): w is BookScreenItem => Boolean(w));
});

const wishWorks = computed<BookScreenItem[]>(() => {
  // Унікальний список (твір може бути одночасно в "хочу прочитати" і "хочу переглянути").
  const map = new Map<string, BookScreenItem>();
  for (const w of [...readWorks.value, ...watchWorks.value]) map.set(w.id, w);
  return Array.from(map.values());
});

// ── Статистика ───────────────────────────────────────────────
const stats = computed(() => ({
  reviewsCount: myReviews.value.length,
  ratingsCount: userRatings.ratings.length,
  watchedCount: watchWorks.value.length,
}));

// ── Helpers для відображення ─────────────────────────────────
const initial = computed(() =>
  (userStore.username || userStore.nickname || userStore.email || '?').charAt(0).toUpperCase()
);

const joinedDateLabel = computed(() => {
  if (!userStore.joinedAt) return '';
  const d = new Date(userStore.joinedAt);
  if (isNaN(d.getTime())) return '';
  return d.toLocaleDateString('uk-UA', { day: '2-digit', month: '2-digit', year: 'numeric' });
});

function findWork(id: string): BookScreenItem | undefined {
  return allWorks.value.find((w) => w.id === id);
}

// ── Edit profile ─────────────────────────────────────────────
function startEditing(): void {
  editForm.value = {
    username: userStore.username,
    nickname: userStore.nickname,
  };
  editing.value = true;
}

async function saveProfile(): Promise<void> {
  try {
    // PUT /api/v1/users/me з полем username.
    await userStore.updateProfile({
      username: editForm.value.username.trim(),
    });
    editing.value = false;
    notifications.pushSuccess(t.profileUpdated);
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  }
}

function cancelEditing(): void {
  editing.value = false;
}

// ── Avatar (mock через FileReader → base64 у localStorage) ───
const avatarInput = ref<HTMLInputElement | null>(null);

function pickAvatar(): void {
  avatarInput.value?.click();
}

// MH-010: використовуємо setAvatar → POST /api/v1/users/me/avatar замість updateProfile
function onAvatarChange(e: Event): void {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  if (file.size > 1024 * 1024) {
    notifications.pushWarning(t.avatarTooLarge);
    return;
  }
  const reader = new FileReader();
  reader.onload = async () => {
    if (typeof reader.result === 'string') {
      try {
        await userStore.setAvatar(reader.result);
        notifications.pushSuccess(t.avatarUpdated);
      } catch (err) {
        notifications.pushError(extractErrorMessage(err));
      }
    }
  };
  reader.readAsDataURL(file);
}

// ── Reviews actions ──────────────────────────────────────────
async function deleteReview(reviewId: string): Promise<void> {
  try {
    await deleteMyReview(reviewId);
    myReviews.value = myReviews.value.filter((r) => r.reviewId !== reviewId);
    notifications.pushSuccess(t.reviewDeleted);
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  }
}

function showMoreReviews(): void {
  visibleReviewsCount.value += REVIEWS_PAGE;
}

// ── Scroll helpers для горизонтальних секцій ─────────────────
const ratingsScroll = ref<HTMLElement | null>(null);
const wishlistScroll = ref<HTMLElement | null>(null);

function scrollSection(el: HTMLElement | null, dir: 'left' | 'right'): void {
  el?.scrollBy({ left: dir === 'left' ? -240 : 240, behavior: 'smooth' });
}

// ── Загальне завантаження ────────────────────────────────────
async function loadAll(): Promise<void> {
  isLoading.value = true;
  try {
    const [worksRes, reviewsRes] = await Promise.all([fetchWorks(), fetchMyReviews()]);
    allWorks.value = worksRes;
    myReviews.value = reviewsRes;
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  } finally {
    isLoading.value = false;
  }
}

onMounted(() => {
  if (!userStore.isAuthenticated) {
    router.push({ name: 'home' });
    return;
  }
  loadAll();
});
</script>

<template>
  <div v-if="userStore.isAuthenticated" class="profile">
    <h1 class="profile__title">{{ t.title }}</h1>

    <!-- ═══════════════ Шапка профілю ═══════════════ -->
    <section class="profile-header">
      <!-- Аватар -->
      <div class="profile-header__avatar-col">
        <div class="profile-header__avatar">
          <img v-if="userStore.avatarUrl" :src="userStore.avatarUrl" alt="Аватар" />
          <span v-else class="profile-header__avatar-fallback">{{ initial }}</span>
        </div>
        <button type="button" class="profile-header__change-photo" @click="pickAvatar">
          {{ t.changePhoto }}
        </button>
        <input ref="avatarInput" type="file" accept="image/*" style="display: none" @change="onAvatarChange" />
      </div>

      <!-- Інфо -->
      <div class="profile-card">
        <div v-if="!editing" class="profile-card__body">
          <p class="profile-card__row">
            <span class="profile-card__label">{{ t.usernameTitle }}:</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">{{ t.username }}</span>
            <span class="profile-card__value">{{ userStore.username || userStore.nickname || '—' }}</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">{{ t.joinedAt }}</span>
            <span class="profile-card__value">{{ joinedDateLabel || '—' }}</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">{{ t.email }}</span>
            <span class="profile-card__value">{{ userStore.email }}</span>
          </p>
          <button type="button" class="profile-card__btn" @click="startEditing">{{ STR.common.edit }}</button>
        </div>

        <form v-else class="profile-card__body" @submit.prevent="saveProfile">
          <label class="profile-card__edit-row">
            <span class="profile-card__label">{{ t.name }}</span>
            <input v-model="editForm.username" type="text" class="profile-card__input" />
          </label>
          <div class="profile-card__edit-actions">
            <button type="submit" class="profile-card__btn">{{ STR.common.save }}</button>
            <button type="button" class="profile-card__btn profile-card__btn--cancel" @click="cancelEditing">
              {{ STR.common.cancel }}
            </button>
          </div>
        </form>
      </div>

      <!-- Статистика -->
      <div class="profile-card profile-card--stats">
        <p class="profile-card__stats-title">📊 {{ t.statsTitle }}</p>
        <p class="profile-card__row">
          <span class="profile-card__label">{{ t.statsReviews }}:</span>
          <span class="profile-card__value">{{ stats.reviewsCount }}</span>
        </p>
        <p class="profile-card__row">
          <span class="profile-card__label">{{ t.statsRatings }}:</span>
          <span class="profile-card__value">{{ stats.ratingsCount }}</span>
        </p>
        <p class="profile-card__row">
          <span class="profile-card__label">{{ t.statsWatched }}:</span>
          <span class="profile-card__value">{{ stats.watchedCount }}</span>
        </p>
      </div>
    </section>

    <!-- ═══════════════ Мої оцінки ═══════════════ -->
    <section class="profile-section">
      <h2 class="profile-section__title">⭐ {{ t.myRatingsTitle }}</h2>
      <p v-if="ratedWorks.length === 0" class="profile-section__empty">{{ t.noRatings }}</p>
      <div v-else class="profile-section__carousel">
        <button class="profile-section__arrow" aria-label="Ліворуч" @click="scrollSection(ratingsScroll, 'left')">
          ←
        </button>
        <div ref="ratingsScroll" class="profile-section__scroll">
          <article v-for="r in ratedWorks" :key="r.work.id" class="rating-card">
            <div class="rating-card__poster">
              <img :src="r.work.poster" :alt="r.work.title" @error="onImgError" />
            </div>
            <div class="rating-card__info">
              <h3 class="rating-card__title">{{ r.work.title }}</h3>
              <dl class="rating-card__meta">
                <div>
                  <dt>{{ STR.detail.bookYear }}:</dt>
                  <dd>{{ r.work.year }}</dd>
                </div>
                <div>
                  <dt>{{ STR.detail.bookGenre }}:</dt>
                  <dd>{{ r.work.genre }}</dd>
                </div>
                <div>
                  <dt>{{ STR.detail.bookCountry }}:</dt>
                  <dd>{{ r.work.country }}</dd>
                </div>
              </dl>
            </div>
            <button
              type="button"
              class="rating-card__btn"
              @click="router.push({ name: 'detail', params: { id: r.work.id } })"
            >
              {{ t.view }}
            </button>
            <div class="rating-card__user-rating">
              <span class="rating-card__user-label">{{ t.yourRating }}</span>
              <StarRating :model-value="Math.max(r.bookRating, r.filmRating)" readonly :size="20" />
            </div>
          </article>
        </div>
        <button class="profile-section__arrow" aria-label="Праворуч" @click="scrollSection(ratingsScroll, 'right')">
          →
        </button>
      </div>
    </section>

    <!-- ═══════════════ Мої відгуки ═══════════════ -->
    <section class="profile-section">
      <h2 class="profile-section__title">💬 {{ t.myReviewsTitle }}</h2>
      <p v-if="myReviews.length === 0" class="profile-section__empty">{{ t.noReviews }}</p>
      <ul v-else class="profile-reviews">
        <li v-for="r in visibleReviews" :key="r.reviewId" class="profile-review">
          <!-- Постер + назва під ним -->
          <div class="profile-review__poster-col">
            <div class="profile-review__poster">
              <img
                v-if="findWork(r.workId)"
                :src="findWork(r.workId)!.poster"
                :alt="findWork(r.workId)!.title"
                @error="onImgError"
              />
            </div>
            <p class="profile-review__work-title">{{ findWork(r.workId)?.title || 'Твір' }}</p>
          </div>
          <!-- Текст відгуку -->
          <div class="profile-review__body">
            <p class="profile-review__label">{{ t.reviewText }}:</p>
            <p class="profile-review__text">{{ r.text }}</p>
          </div>
          <!-- Кнопки -->
          <div class="profile-review__actions">
            <button
              type="button"
              class="profile-review__btn profile-review__btn--edit"
              @click="router.push({ name: 'detail', params: { id: r.workId } })"
            >
              {{ STR.common.edit }}
            </button>
            <button
              type="button"
              class="profile-review__btn profile-review__btn--delete"
              @click="deleteReview(r.reviewId)"
            >
              {{ STR.common.delete }}
            </button>
          </div>
        </li>
      </ul>
      <button v-if="hasMoreReviews" type="button" class="profile-show-more" @click="showMoreReviews">
        {{ STR.common.showMore }}
      </button>
    </section>

    <!-- ═══════════════ Список "Хочу переглянути/прочитати" ═══════════════ -->
    <section class="profile-section">
      <h2 class="profile-section__title">🔖 {{ t.wishlistTitle }}</h2>
      <p v-if="wishWorks.length === 0" class="profile-section__empty">{{ t.noWishlist }}</p>
      <div v-else class="profile-section__carousel">
        <button class="profile-section__arrow" aria-label="Ліворуч" @click="scrollSection(wishlistScroll, 'left')">
          ←
        </button>
        <div ref="wishlistScroll" class="profile-section__scroll">
          <WorkCard v-for="w in wishWorks" :key="w.id" :item="w" />
        </div>
        <button class="profile-section__arrow" aria-label="Праворуч" @click="scrollSection(wishlistScroll, 'right')">
          →
        </button>
      </div>
    </section>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Comfortaa:wght@400;500;600;700&display=swap');

/* ── Сторінка ───────────────────────────────────────────── */
.profile {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 24px 20px 48px;
  font-family: 'Comfortaa', var(--font-body), sans-serif;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}

.profile__title {
  margin: 0;
  text-align: center;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 26px;
  font-weight: 600;
  color: var(--text-on-light);
}

/* ── Шапка профілю ─────────────────────────────────────── */
.profile-header {
  background: var(--color-panel);
  border-radius: 14px;
  padding: 28px 24px;
  display: grid;
  grid-template-columns: 140px 1fr 220px;
  gap: 20px;
  align-items: stretch;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  min-height: 180px;
}

/* Аватар */
.profile-header__avatar-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.profile-header__avatar {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  overflow: hidden;
  background: var(--color-card);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.profile-header__avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile-header__avatar-fallback {
  font-size: 40px;
  color: var(--text-on-dark);
}

.profile-header__change-photo {
  background: none;
  border: none;
  color: var(--color-primary);
  font-family: 'Comfortaa', sans-serif;
  font-size: 12px;
  text-decoration: underline;
  cursor: pointer;
  padding: 0;
}

/* Картка інфо */
.profile-card {
  background: #95696b;
  color: #fff;
  border-radius: 10px;
  padding: 18px 22px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  height: 100%;
  box-sizing: border-box;
}

.profile-card__body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 13px;
  height: 100%;
}

.profile-card__row {
  margin: 0;
  display: flex;
  gap: 6px;
  align-items: baseline;
  flex-wrap: wrap;
}

.profile-card__label {
  color: rgba(255, 255, 255, 0.65);
  font-size: 13px;
  white-space: nowrap;
}

.profile-card__value {
  color: #fff;
  font-weight: 600;
  font-size: 13px;
}

.profile-card__btn {
  align-self: flex-start;
  margin-top: auto;
  background: var(--color-primary);
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 7px 20px;
  font-family: 'Comfortaa', sans-serif;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  transition: background 0.15s;
}

.profile-card__btn:hover {
  background: var(--color-primary-hover);
}

.profile-card__btn--cancel {
  background: transparent;
  border: 2px solid rgba(255, 255, 255, 0.5);
}

.profile-card__btn--cancel:hover {
  background: rgba(255, 255, 255, 0.15);
}

.profile-card__edit-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile-card__input {
  padding: 6px 10px;
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 6px;
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  font-family: 'Comfortaa', sans-serif;
  font-size: 13px;
  outline: none;
}

.profile-card__edit-actions {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}

/* Картка статистики */
.profile-card--stats {
  align-items: flex-start;
}

.profile-card__stats-title {
  margin: 0 0 6px;
  font-size: 13px;
  font-weight: 700;
  color: #fff;
}

/* ── Секції ────────────────────────────────────────────── */
.profile-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
  background: var(--color-panel);
  border-radius: 14px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.profile-section__title {
  margin: 0;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 17px;
  font-weight: 700;
  color: var(--text-on-light);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.profile-section__empty {
  margin: 0;
  text-align: center;
  font-size: 14px;
  color: var(--text-muted);
  padding: 16px;
}

/* Карусель зі стрілками */
.profile-section__carousel {
  display: flex;
  align-items: center;
  gap: 8px;
}

.profile-section__arrow {
  flex-shrink: 0;
  background: #f5c5ca;
  color: #1a0a0a;
  border: none;
  border-radius: 50%;
  width: 38px;
  height: 38px;
  font-size: 18px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  transition:
    background 0.15s,
    box-shadow 0.15s;
  padding: 0;
  line-height: 1;
}

.profile-section__arrow:hover {
  background: #f0aab2;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.profile-section__scroll {
  flex: 1;
  display: flex;
  gap: 14px;
  overflow-x: auto;
  padding-bottom: 6px;
  scrollbar-width: none;
}

.profile-section__scroll::-webkit-scrollbar {
  display: none;
}

/* ── Картка з оцінкою ──────────────────────────────────── */
.rating-card {
  flex-shrink: 0;
  width: 220px;
  height: 420px;
  background: var(--color-card);
  border-radius: 10px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  color: var(--text-on-dark);
  box-sizing: border-box;
}

.rating-card__info {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.rating-card__poster {
  width: 100%;
  height: 190px;
  overflow: hidden;
  border-radius: 8px;
  background: var(--color-header);
  flex-shrink: 0;
}

.rating-card__poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.rating-card__title {
  margin: 0;
  font-size: 12px;
  text-align: center;
  font-weight: 600;
  line-height: 1.3;
  height: 32px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.rating-card__meta {
  margin: 0;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 11px;
}

.rating-card__meta div {
  display: flex;
  gap: 4px;
}

.rating-card__meta dt {
  color: rgba(255, 255, 255, 0.6);
  white-space: nowrap;
}

.rating-card__meta dd {
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  flex: 1;
}

.rating-card__btn {
  width: 100%;
  height: 32px;
  background: var(--color-primary);
  color: #fff;
  border: none;
  border-radius: 7px;
  padding: 0;
  font-family: 'Comfortaa', sans-serif;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.rating-card__btn:hover {
  background: var(--color-primary-hover);
}

.rating-card__user-rating {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
  flex-shrink: 0;
  height: 36px;
}

.rating-card__user-label {
  font-size: 11px;
  color: rgba(255, 255, 255, 0.7);
}

/* ── Мої відгуки ───────────────────────────────────────── */
.profile-reviews {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.profile-review {
  display: grid;
  grid-template-columns: 110px 1fr auto;
  gap: 16px;
  align-items: center;
  background: var(--color-card);
  color: #fff;
  border-radius: 12px;
  padding: 14px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.18);
}

/* Ліва колонка: постер + назва під ним */
.profile-review__poster-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.profile-review__poster {
  width: 90px;
  height: 120px;
  overflow: hidden;
  border-radius: 6px;
  background: var(--color-header);
  flex-shrink: 0;
}

.profile-review__poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile-review__work-title {
  margin: 0;
  font-size: 11px;
  text-align: center;
  color: rgba(255, 255, 255, 0.85);
  line-height: 1.3;
}

/* Центр: текст */
.profile-review__body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.profile-review__label {
  margin: 0;
  font-size: 14px;
  color: #fff;
  font-weight: 600;
}

.profile-review__text {
  margin: 0;
  font-size: 13px;
  word-break: break-word;
  color: rgba(255, 255, 255, 0.75);
}

/* Праворуч: кнопки */
.profile-review__actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile-review__btn {
  border: none;
  border-radius: 7px;
  padding: 8px 18px;
  font-family: 'Comfortaa', sans-serif;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s;
}

.profile-review__btn--edit {
  background: var(--color-primary);
  color: #fff;
}

.profile-review__btn--edit:hover {
  background: var(--color-primary-hover);
}

.profile-review__btn--delete {
  background: var(--color-header);
  color: #fff;
}

.profile-review__btn--delete:hover {
  background: var(--color-primary);
}

/* Кнопка "Показати більше" */
.profile-show-more {
  align-self: center;
  background: var(--color-card);
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 10px 40px;
  font-family: 'Comfortaa', sans-serif;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  transition: background 0.15s;
}

.profile-show-more:hover {
  background: var(--color-primary);
}

/* ── Адаптив ───────────────────────────────────────────── */
@media (max-width: 700px) {
  .profile-header {
    grid-template-columns: 1fr;
    text-align: center;
  }
  .profile-header__avatar-col {
    align-items: center;
  }
  .profile-card__btn {
    align-self: center;
  }
}

@media (max-width: 500px) {
  .profile-review {
    grid-template-columns: 80px 1fr;
  }
  .profile-review__actions {
    grid-column: 1 / -1;
    flex-direction: row;
    justify-content: flex-end;
  }
  .profile-review__poster-col {
    flex-direction: row;
    align-items: flex-start;
  }
}
</style>
