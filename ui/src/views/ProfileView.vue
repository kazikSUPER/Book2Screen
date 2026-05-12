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
  fullName: '',
  nickname: '',
  birthDate: '',
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
  (userStore.fullName || userStore.nickname || userStore.email || '?').charAt(0).toUpperCase()
);

function findWork(id: string): BookScreenItem | undefined {
  return allWorks.value.find((w) => w.id === id);
}

// ── Edit profile ─────────────────────────────────────────────
function startEditing(): void {
  editForm.value = {
    fullName: userStore.fullName,
    nickname: userStore.nickname,
    birthDate: userStore.birthDate,
  };
  editing.value = true;
}

function saveProfile(): void {
  userStore.updateProfile({
    fullName: editForm.value.fullName.trim(),
    nickname: editForm.value.nickname.trim(),
    birthDate: editForm.value.birthDate.trim(),
  });
  editing.value = false;
  notifications.pushSuccess('Профіль оновлено');
}

function cancelEditing(): void {
  editing.value = false;
}

// ── Avatar (mock через FileReader → base64 у localStorage) ───
const avatarInput = ref<HTMLInputElement | null>(null);

function pickAvatar(): void {
  avatarInput.value?.click();
}

function onAvatarChange(e: Event): void {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  if (file.size > 1024 * 1024) {
    notifications.pushWarning('Розмір файлу не має перевищувати 1 МБ');
    return;
  }
  const reader = new FileReader();
  reader.onload = () => {
    if (typeof reader.result === 'string') {
      userStore.updateProfile({ avatarUrl: reader.result });
      notifications.pushSuccess('Фото оновлено');
    }
  };
  reader.readAsDataURL(file);
}

// ── Reviews actions ──────────────────────────────────────────
async function deleteReview(reviewId: string): Promise<void> {
  try {
    await deleteMyReview(reviewId);
    myReviews.value = myReviews.value.filter((r) => r.reviewId !== reviewId);
    notifications.pushSuccess('Відгук видалено');
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  }
}

function showMoreReviews(): void {
  visibleReviewsCount.value += REVIEWS_PAGE;
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
    <h1 class="profile__title">Ваш профіль</h1>

    <!-- ═════════════ Шапка профілю ═════════════ -->
    <section class="profile-header">
      <!-- Аватар -->
      <div class="profile-header__avatar-col">
        <div class="profile-header__avatar">
          <img v-if="userStore.avatarUrl" :src="userStore.avatarUrl" alt="Аватар" />
          <span v-else class="profile-header__avatar-fallback">{{ initial }}</span>
        </div>
        <button type="button" class="profile-header__change-photo" @click="pickAvatar">
          Змінити фото
        </button>
        <input
          ref="avatarInput"
          type="file"
          accept="image/*"
          style="display: none"
          @change="onAvatarChange"
        />
      </div>

      <!-- Інформація про користувача -->
      <div class="profile-card">
        <header class="profile-card__head">
          <span class="profile-card__icon" aria-hidden="true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
              <circle cx="12" cy="8" r="4" />
              <path d="M4 21C4 16.5817 7.58172 13 12 13C16.4183 13 20 16.5817 20 21V22H4V21Z" />
            </svg>
          </span>
          <h2 class="profile-card__title">Ім'я користувача:</h2>
        </header>

        <div v-if="!editing" class="profile-card__body">
          <p class="profile-card__row">
            <span class="profile-card__label">Username:</span>
            <span class="profile-card__value">{{ userStore.fullName || userStore.nickname || '—' }}</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">Дата народження:</span>
            <span class="profile-card__value">{{ userStore.birthDate || '—' }}</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">Електронна пошта:</span>
            <span class="profile-card__value">{{ userStore.email }}</span>
          </p>
          <button type="button" class="profile-card__edit" @click="startEditing">Редагувати</button>
        </div>

        <form v-else class="profile-card__body" @submit.prevent="saveProfile">
          <label class="profile-card__edit-row">
            <span class="profile-card__label">Ім'я</span>
            <input v-model="editForm.fullName" type="text" class="profile-card__input" />
          </label>
          <label class="profile-card__edit-row">
            <span class="profile-card__label">Username</span>
            <input v-model="editForm.nickname" type="text" class="profile-card__input" />
          </label>
          <label class="profile-card__edit-row">
            <span class="profile-card__label">Дата народження</span>
            <input v-model="editForm.birthDate" type="date" class="profile-card__input" />
          </label>
          <div class="profile-card__edit-actions">
            <button type="submit" class="profile-card__edit">Зберегти</button>
            <button type="button" class="profile-card__cancel" @click="cancelEditing">Скасувати</button>
          </div>
        </form>
      </div>

      <!-- Статистика -->
      <div class="profile-card profile-card--stats">
        <header class="profile-card__head">
          <span class="profile-card__icon" aria-hidden="true">📊</span>
          <h2 class="profile-card__title">Статистика активності:</h2>
        </header>
        <div class="profile-card__body">
          <p class="profile-card__row">
            <span class="profile-card__label">Відгуки:</span>
            <span class="profile-card__value">{{ stats.reviewsCount }}</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">Оцінки:</span>
            <span class="profile-card__value">{{ stats.ratingsCount }}</span>
          </p>
          <p class="profile-card__row">
            <span class="profile-card__label">Переглянуто:</span>
            <span class="profile-card__value">{{ stats.watchedCount }}</span>
          </p>
        </div>
      </div>
    </section>

    <!-- ═════════════ Мої оцінки ═════════════ -->
    <section class="profile-section">
      <h2 class="profile-section__title">⭐ Мої оцінки</h2>
      <p v-if="ratedWorks.length === 0" class="profile-section__empty">
        Ви ще не поставили жодної оцінки
      </p>
      <div v-else class="profile-section__scroll">
        <article v-for="r in ratedWorks" :key="r.work.id" class="rating-card">
          <div class="rating-card__poster">
            <img :src="r.work.poster" :alt="r.work.title" />
          </div>
          <h3 class="rating-card__title">{{ r.work.title }}</h3>
          <dl class="rating-card__meta">
            <div><dt>Рік:</dt><dd>{{ r.work.year }}</dd></div>
            <div><dt>Жанр:</dt><dd>{{ r.work.genre }}</dd></div>
            <div><dt>Країна:</dt><dd>{{ r.work.country }}</dd></div>
          </dl>
          <button class="rating-card__btn" @click="router.push({ name: 'detail', params: { id: r.work.id } })">
            Переглянути
          </button>
          <div class="rating-card__user-rating">
            <span class="rating-card__user-label">Ваша оцінка</span>
            <StarRating :model-value="Math.max(r.bookRating, r.filmRating)" readonly :size="20" />
          </div>
        </article>
      </div>
    </section>

    <!-- ═════════════ Мої відгуки ═════════════ -->
    <section class="profile-section">
      <h2 class="profile-section__title">💬 Мої відгуки</h2>
      <p v-if="myReviews.length === 0" class="profile-section__empty">У вас ще немає відгуків</p>
      <ul v-else class="profile-reviews">
        <li v-for="r in visibleReviews" :key="r.reviewId" class="profile-review">
          <div class="profile-review__poster">
            <img v-if="findWork(r.workId)" :src="findWork(r.workId)!.poster" :alt="findWork(r.workId)!.title" />
          </div>
          <div class="profile-review__body">
            <p class="profile-review__work">{{ findWork(r.workId)?.title || 'Твір' }}</p>
            <p class="profile-review__label">Текст відгуку:</p>
            <p class="profile-review__text">{{ r.text }}</p>
          </div>
          <div class="profile-review__actions">
            <button
              class="profile-review__btn profile-review__btn--edit"
              @click="router.push({ name: 'detail', params: { id: r.workId } })"
            >
              Редагувати
            </button>
            <button class="profile-review__btn profile-review__btn--delete" @click="deleteReview(r.reviewId)">
              Видалити
            </button>
          </div>
        </li>
      </ul>
      <button v-if="hasMoreReviews" type="button" class="profile-show-more" @click="showMoreReviews">
        Показати більше
      </button>
    </section>

    <!-- ═════════════ Список "Хочу переглянути/прочитати" ═════════════ -->
    <section class="profile-section">
      <h2 class="profile-section__title">Список «Хочу переглянути/прочитати»</h2>
      <p v-if="wishWorks.length === 0" class="profile-section__empty">
        Ви ще нічого не додали в список
      </p>
      <div v-else class="profile-section__scroll">
        <WorkCard v-for="w in wishWorks" :key="w.id" :item="w" />
      </div>
    </section>
  </div>
</template>

<style scoped>
.profile {
  display: flex;
  flex-direction: column;
  gap: 32px;
  padding: 24px 16px 48px;
  font-family: var(--font-body);
}

.profile__title {
  margin: 0;
  text-align: center;
  font-family: var(--font-display);
  font-size: 28px;
  font-weight: 400;
  color: var(--text-on-light);
}

/* ── Шапка профілю ─────────────────────────────────────── */
.profile-header {
  background: var(--color-panel-box);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  padding: 24px;
  display: grid;
  grid-template-columns: auto 1fr 1fr;
  gap: 24px;
  align-items: stretch;
  box-shadow: var(--shadow-sm);
}

.profile-header__avatar-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.profile-header__avatar {
  width: 110px;
  height: 110px;
  border-radius: var(--radius-pill);
  overflow: hidden;
  background: var(--color-card);
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile-header__avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile-header__avatar-fallback {
  font-family: var(--font-display);
  font-size: 48px;
  color: var(--text-on-dark);
}

.profile-header__change-photo {
  background: none;
  border: none;
  color: var(--color-primary);
  font-family: var(--font-display);
  font-size: 13px;
  text-decoration: underline;
  cursor: pointer;
  padding: 0;
}

.profile-header__change-photo:hover {
  color: var(--color-primary-hover);
}

/* ── Картки info / stats ───────────────────────────────── */
.profile-card {
  background: var(--color-card);
  color: var(--text-on-dark);
  border-radius: var(--radius-md);
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: var(--shadow-sm);
}

.profile-card__head {
  display: flex;
  align-items: center;
  gap: 8px;
}

.profile-card__icon {
  display: inline-flex;
  color: var(--text-on-dark);
}

.profile-card__title {
  margin: 0;
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 500;
}

.profile-card__body {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 13px;
}

.profile-card__row {
  margin: 0;
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.profile-card__label {
  color: var(--color-panel-box);
  font-family: var(--font-body);
}

.profile-card__value {
  color: var(--text-on-dark);
  font-weight: 500;
}

.profile-card__edit {
  align-self: flex-start;
  margin-top: 8px;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-sm);
  padding: 6px 16px;
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
}

.profile-card__edit:hover {
  background: var(--color-primary-hover);
}

.profile-card__cancel {
  align-self: flex-start;
  background: transparent;
  color: var(--text-on-dark);
  border: 1px solid var(--text-on-dark);
  border-radius: var(--radius-sm);
  padding: 6px 16px;
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
}

.profile-card__edit-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile-card__input {
  padding: 6px 8px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-xs);
  background: var(--color-input-bg);
  color: var(--text-on-light);
  font-family: var(--font-body);
  font-size: 13px;
  outline: none;
}

.profile-card__edit-actions {
  display: flex;
  gap: 8px;
}

/* ── Секції ────────────────────────────────────────────── */
.profile-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: var(--color-panel-box);
  border-radius: var(--radius-md);
  padding: 16px;
  border: 1px solid var(--border-default);
  box-shadow: var(--shadow-sm);
}

.profile-section__title {
  margin: 0;
  font-family: var(--font-display);
  font-size: 18px;
  color: var(--text-on-light);
  font-weight: 400;
}

.profile-section__empty {
  margin: 0;
  text-align: center;
  font-family: var(--font-body);
  font-size: 14px;
  color: var(--text-muted);
  padding: 16px;
}

.profile-section__scroll {
  display: flex;
  gap: 16px;
  overflow-x: auto;
  padding-bottom: 8px;
  scrollbar-width: thin;
  scrollbar-color: var(--color-primary) transparent;
}

.profile-section__scroll::-webkit-scrollbar {
  height: 6px;
}

.profile-section__scroll::-webkit-scrollbar-thumb {
  background: var(--color-primary);
  border-radius: 3px;
}

/* ── Картка з оцінкою (компактна) ──────────────────────── */
.rating-card {
  flex-shrink: 0;
  width: 200px;
  background: var(--color-card);
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  padding: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  box-shadow: var(--shadow-sm);
  color: var(--text-on-dark);
}

.rating-card__poster {
  width: 100%;
  height: 200px;
  overflow: hidden;
  border-radius: var(--radius-xs);
  background: var(--color-header);
}

.rating-card__poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.rating-card__title {
  margin: 0;
  font-family: var(--font-body);
  font-size: 13px;
  text-align: center;
  font-weight: 500;
}

.rating-card__meta {
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
  font-size: 11px;
  align-self: flex-start;
}

.rating-card__meta div {
  display: flex;
  gap: 4px;
}

.rating-card__btn {
  width: 100%;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-sm);
  padding: 6px;
  font-family: var(--font-display);
  font-size: 12px;
  cursor: pointer;
}

.rating-card__btn:hover {
  background: var(--color-primary-hover);
}

.rating-card__user-rating {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.rating-card__user-label {
  font-family: var(--font-display);
  font-size: 11px;
}

/* ── Мої відгуки ───────────────────────────────────────── */
.profile-reviews {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.profile-review {
  display: grid;
  grid-template-columns: 80px 1fr auto;
  gap: 12px;
  align-items: stretch;
  background: var(--color-card);
  color: var(--text-on-dark);
  border-radius: var(--radius-sm);
  padding: 12px;
}

.profile-review__poster {
  width: 80px;
  height: 110px;
  overflow: hidden;
  border-radius: var(--radius-xs);
  background: var(--color-header);
}

.profile-review__poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile-review__body {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.profile-review__work {
  margin: 0;
  font-family: var(--font-display);
  font-size: 13px;
  color: var(--color-panel-box);
}

.profile-review__label {
  margin: 0;
  font-family: var(--font-display);
  font-size: 12px;
  color: var(--color-panel-box);
}

.profile-review__text {
  margin: 0;
  font-family: var(--font-body);
  font-size: 13px;
  word-break: break-word;
}

.profile-review__actions {
  display: flex;
  flex-direction: column;
  gap: 6px;
  align-self: center;
}

.profile-review__btn {
  border: none;
  border-radius: var(--radius-sm);
  padding: 6px 14px;
  font-family: var(--font-display);
  font-size: 12px;
  cursor: pointer;
  white-space: nowrap;
}

.profile-review__btn--edit {
  background: var(--color-primary);
  color: var(--text-on-primary);
}

.profile-review__btn--edit:hover {
  background: var(--color-primary-hover);
}

.profile-review__btn--delete {
  background: var(--color-header);
  color: var(--text-on-dark);
}

.profile-review__btn--delete:hover {
  background: var(--color-primary);
}

.profile-show-more {
  align-self: center;
  background: transparent;
  color: var(--text-on-light);
  border: 1px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 6px 32px;
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
}

.profile-show-more:hover {
  background: var(--color-card);
  color: var(--text-on-dark);
}

/* ── Адаптив ───────────────────────────────────────────── */
@media (max-width: 900px) {
  .profile-header {
    grid-template-columns: 1fr;
    gap: 16px;
    text-align: center;
  }
  .profile-header__avatar-col {
    align-items: center;
  }
}

@media (max-width: 600px) {
  .profile-review {
    grid-template-columns: 60px 1fr;
  }
  .profile-review__actions {
    grid-column: 1 / -1;
    flex-direction: row;
    justify-content: flex-end;
  }
  .profile-review__poster {
    width: 60px;
    height: 80px;
  }
}
</style>
