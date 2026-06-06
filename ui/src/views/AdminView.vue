<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../state/user';
import { useNotificationsStore } from '../state/notifications';
import {
  fetchAllBooks,
  createBook,
  updateBook,
  deleteBook,
  fetchReports,
  moderateReport,
  type ReportedComment,
} from '../services/admin';
import type { BookScreenItem, DifferencePoint } from '../services/types';
import { extractErrorMessage } from '../services/error';
import { GENRES, STR } from '../constants';
import { onImgError } from '../composables/useImageFallback';

/**
 * SCRUM-143 — Admin Panel.
 *
 * Три режими (mode):
 *   1. 'books'     — таблиця всіх творів + пошук + дії "Редагувати/Видалити".
 *                    Зліва бокс з кнопками "Додати книгу" / "Модерація коментарів"
 *                    і вибрана картка-прев'ю.
 *   2. 'comments'  — таблиця скарг на коментарі: Причина / Коментар / Дія
 *                    (Схвалити / Видалити / Спойлер).
 *   3. 'book-form' — форма додавання/редагування твору з інтерактивним
 *                    редактором карти відмінностей.
 *
 * Доступ: тільки залогіненим (поки backend не повертає role у JWT).
 * TODO: коли в LoginResponse з'явиться role — додати if (role !== 'admin').
 */

const router = useRouter();
const userStore = useUserStore();
const notifications = useNotificationsStore();

type Mode = 'books' | 'comments' | 'book-form';
const mode = ref<Mode>('books');

// ── Списки ──────────────────────────────────────────────────
const books = ref<BookScreenItem[]>([]);
const reports = ref<ReportedComment[]>([]);
const isLoading = ref(false);

// Пошук у списку книг.
const searchQuery = ref('');

const filteredBooks = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return books.value;
  return books.value.filter((b) =>
    [b.title, b.author, String(b.year)].filter(Boolean).some((s) => s!.toLowerCase().includes(q))
  );
});

const selectedBook = ref<BookScreenItem | null>(null);

// ── Форма додавання/редагування ─────────────────────────────

/** Додаткова адаптація — для кнопки "Додати екранізацію". */
interface ExtraAdaptation {
  id: string; // тимчасовий клієнтський id
  type: string; // movie | series | anime
  releaseYear: number | null;
  posterUrl: string;
  studio: string;
  country: string;
}

interface BookForm {
  id: string | null; // null = create
  title: string;
  author: string;
  year: number | null;
  genre: string;
  country: string;
  poster: string;
  bookRating: number;
  filmRating: number;
  description: string;
  hasMap: boolean;
  differences: DifferencePoint[];
  // BUG-045: бек вимагає type = 'movie' | 'series' | 'anime'.
  type: string;
  /** Список додаткових адаптацій (опціонально — серіал + фільм). */
  extraAdaptations: ExtraAdaptation[];
}

const emptyForm = (): BookForm => ({
  id: null,
  title: '',
  author: '',
  year: null,
  genre: '',
  country: '',
  poster: '',
  bookRating: 0,
  filmRating: 0,
  description: '',
  hasMap: false,
  differences: [],
  type: 'movie',
  extraAdaptations: [],
});

import { usePersistedRef } from '../composables/usePersistedRef';

// Додає блок ще однієї екранізації під формою.
function addAdaptation(): void {
  form.value.extraAdaptations.push({
    id: `adapt-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
    type: 'series',
    releaseYear: null,
    posterUrl: '',
    studio: '',
    country: '',
  });
}

function removeAdaptation(index: number): void {
  form.value.extraAdaptations.splice(index, 1);
}

// BUG-048: зберігаємо стан форми у sessionStorage, щоб дані не зникали при випадковому перемиканні вкладок.
const form = usePersistedRef<BookForm>('admin_book_draft', emptyForm(), sessionStorage);
const isSubmitting = ref(false);

const genreOptions = GENRES;
const t = STR.admin;

// ── Завантаження даних ──────────────────────────────────────
async function loadBooks(): Promise<void> {
  isLoading.value = true;
  try {
    books.value = await fetchAllBooks();
    if (!selectedBook.value && books.value.length > 0) {
      selectedBook.value = books.value[0];
    }
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  } finally {
    isLoading.value = false;
  }
}

async function loadReports(): Promise<void> {
  isLoading.value = true;
  try {
    reports.value = await fetchReports();
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  } finally {
    isLoading.value = false;
  }
}

// ── Mode switchers ──────────────────────────────────────────
function switchToBooks(): void {
  mode.value = 'books';
}

function switchToComments(): void {
  mode.value = 'comments';
  if (reports.value.length === 0) loadReports();
}

function startCreate(): void {
  form.value = emptyForm();
  mode.value = 'book-form';
}

function startEdit(book: BookScreenItem): void {
  form.value = {
    id: book.id,
    title: book.title,
    author: book.author ?? '',
    year: book.year,
    genre: book.genre,
    country: book.country,
    poster: book.poster,
    bookRating: book.bookRating,
    filmRating: book.filmRating,
    description: book.description,
    hasMap: book.hasMap ?? false,
    differences: book.differences ? JSON.parse(JSON.stringify(book.differences)) : [],
    type: 'movie', // BUG-045: бек не повертає type у /Works/{id}, тому дефолт.
    extraAdaptations: [],
  };
  mode.value = 'book-form';
}

// ── CRUD ────────────────────────────────────────────────────
async function onDelete(book: BookScreenItem): Promise<void> {
  if (!confirm(t.confirmDelete(book.title))) return;
  try {
    await deleteBook(book.id);
    books.value = books.value.filter((b) => b.id !== book.id);
    if (selectedBook.value?.id === book.id) selectedBook.value = books.value[0] ?? null;
    notifications.pushSuccess(t.bookDeleted);
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  }
}

async function submitForm(): Promise<void> {
  // BUG-045: type теж обов'язковий поряд із title і year.
  if (!form.value.title || !form.value.year || !form.value.type) {
    notifications.pushWarning(t.fillTitleYearType);
    return;
  }
  isSubmitting.value = true;
  try {
    // BUG-045: передаємо type в admin.createBook/updateBook (мапиться в AdaptationDto.type).
    const payload = {
      title: form.value.title,
      author: form.value.author || undefined,
      year: form.value.year ?? 0,
      genre: form.value.genre,
      country: form.value.country,
      poster: form.value.poster,
      bookRating: form.value.bookRating,
      filmRating: form.value.filmRating,
      description: form.value.description,
      hasMap: form.value.differences.length > 0,
      differences: form.value.differences.length > 0 ? form.value.differences : undefined,
      type: form.value.type,
    };
    if (form.value.id) {
      const updated = await updateBook(form.value.id, payload);
      const idx = books.value.findIndex((b) => b.id === updated.id);
      if (idx >= 0) books.value[idx] = updated;
      notifications.pushSuccess(t.bookUpdated);
    } else {
      const created = await createBook(payload);
      books.value.push(created);
      notifications.pushSuccess(t.bookAdded);
    }
    mode.value = 'books';
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  } finally {
    isSubmitting.value = false;
  }
}

/**
 * BUG-046: підтвердження перед скасуванням форми.
 * Запитуємо тільки якщо форма має непорожні поля — щоб не дратувати при чистому формі.
 */
function cancelForm(): void {
  const isDirty =
    form.value.title.trim() !== '' ||
    form.value.author.trim() !== '' ||
    form.value.year !== null ||
    form.value.genre !== '' ||
    form.value.country.trim() !== '' ||
    form.value.poster.trim() !== '' ||
    form.value.description.trim() !== '' ||
    form.value.differences.length > 0;

  if (isDirty && !confirm(t.confirmCancelForm)) return;
  mode.value = 'books';
}

// ── Differences map editor ──────────────────────────────────
function addPoint(): void {
  form.value.differences.push({
    id: `new-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
    title: '',
    bookText: '',
    filmText: '',
    isSpoiler: false,
  });
}

function removePoint(index: number): void {
  form.value.differences.splice(index, 1);
}

// ── Comment moderation actions (BUG-041) ─────────────────
async function moderate(reportId: string, action: 'delete' | 'dismiss' | 'spoiler'): Promise<void> {
  try {
    await moderateReport(reportId, action);
    // BUG-041: одразу прибираємо запис зі списку, щоб UI оновився
    // (раніше тільки міняли status, а кнопка лишалась видимою з disabled).
    reports.value = reports.value.filter((x) => x.reportId !== reportId);
    const labels = {
      delete: t.commentDeleted,
      dismiss: t.reportRejected,
      spoiler: t.markedSpoiler,
    };
    notifications.pushSuccess(labels[action]);
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  }
}

// ── Mount ───────────────────────────────────────────────────
onMounted(() => {
  if (!userStore.isAuthenticated) {
    router.push({ name: 'home' });
    return;
  }
  loadBooks();
});
</script>

<template>
  <div v-if="userStore.isAuthenticated" class="admin">
    <!-- ═════════ Стрічка-заголовок ═════════ -->
    <div class="admin__stripe">
      <span v-if="mode === 'books'">{{ t.panelTitle }}</span>
      <span v-else-if="mode === 'comments'">{{ t.moderationTitle }}</span>
      <span v-else>{{ form.id ? t.editBookTitle : t.addBookTitle }}</span>
    </div>

    <!-- ═════════ Mode: книги ═════════ -->
    <template v-if="mode === 'books'">
      <div class="admin__top-section">
        <!-- Лівий бокс з діями -->
        <div class="admin__actions-box">
          <button class="admin__action" @click="startCreate">{{ t.addBook }}</button>
          <button class="admin__action" @click="switchToComments">
            {{ t.moderation }}
          </button>
        </div>

        <!-- Правий бокс: пошук + картка-прев'ю -->
        <div class="admin__preview-section">
          <div class="admin__search">
            <input v-model="searchQuery" type="text" :placeholder="t.searchPlaceholder" class="admin__search-input" />
            <span class="admin__search-icon" aria-hidden="true">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
                <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" />
                <path d="M21 21L16.5 16.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              </svg>
            </span>
          </div>

          <article v-if="selectedBook" class="admin__preview-card">
            <div class="admin__preview-content">
              <div class="admin__preview-poster">
                <img :src="selectedBook.poster" :alt="selectedBook.title" @error="onImgError" />
              </div>
              <div class="admin__preview-meta">
                <h3>{{ selectedBook.title }}</h3>
                <p><strong>Рік:</strong> {{ selectedBook.year }}</p>
                <p><strong>Жанр:</strong> {{ selectedBook.genre }}</p>
                <p><strong>Країна:</strong> {{ selectedBook.country }}</p>
                <p v-if="selectedBook.author"><strong>Автор:</strong> {{ selectedBook.author }}</p>
                <p><strong>Рейтинг:</strong> {{ selectedBook.bookRating }} / 10</p>
              </div>
            </div>
            <div class="admin__preview-actions">
              <div class="admin__action-wrapper admin__action-wrapper--left">
                <button class="admin__btn admin__btn--dark" @click="startEdit(selectedBook)">
                  {{ STR.common.edit }}
                </button>
              </div>
              <div class="admin__action-wrapper admin__action-wrapper--right">
                <button class="admin__btn admin__btn--dark" @click="onDelete(selectedBook)">
                  {{ STR.common.delete }}
                </button>
              </div>
            </div>
          </article>
        </div>
      </div>

      <!-- Main: таблиця на всю ширину -->
      <div class="admin__table-section">
        <p v-if="isLoading" class="admin__status">{{ STR.common.loading }}</p>

        <table v-else class="admin__table" aria-label="Список творів">
          <thead>
            <tr>
              <th>{{ t.idHeader }}</th>
              <th>{{ t.titleHeader }}</th>
              <th>{{ t.authorLabel }}</th>
              <th>{{ t.yearHeader }}</th>
              <th>{{ t.actionHeader }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(b, i) in filteredBooks"
              :key="b.id"
              :class="{ 'admin__row--selected': selectedBook?.id === b.id }"
              @click="selectedBook = b"
            >
              <td>{{ i + 1 }}</td>
              <td>{{ b.title }}</td>
              <td>{{ b.author || '—' }}</td>
              <td>{{ b.year }}</td>
              <td class="admin__row-actions" @click.stop>
                <button class="admin__btn admin__btn--primary" @click="startEdit(b)">
                  {{ STR.common.edit }}
                </button>
                <button class="admin__btn admin__btn--dark" @click="onDelete(b)">{{ STR.common.delete }}</button>
              </td>
            </tr>
            <tr v-if="filteredBooks.length === 0">
              <td colspan="5" class="admin__empty">{{ t.nothingFound }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- ═════════ Mode: коментарі ═════════ -->
    <template v-else-if="mode === 'comments'">
      <div class="admin__main">
        <button class="admin__back" type="button" @click="switchToBooks">{{ t.backToBooks }}</button>

        <p v-if="isLoading" class="admin__status">{{ STR.common.loading }}</p>

        <table v-else class="admin__table admin__table--reports">
          <thead>
            <tr>
              <th>{{ t.reasonHeader }}</th>
              <th>{{ t.commentHeader }}</th>
              <th>{{ t.actionHeader }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in reports" :key="r.reportId" :class="`admin__row--${r.status}`">
              <td>{{ r.reason }}</td>
              <td class="admin__report-text">{{ r.review.text }}</td>
              <td class="admin__row-actions">
                <button
                  class="admin__btn admin__btn--ghost"
                  :disabled="r.status !== 'pending'"
                  @click="moderate(r.reportId, 'dismiss')"
                >
                  {{ t.approve }}
                </button>
                <button
                  class="admin__btn admin__btn--primary"
                  :disabled="r.status !== 'pending'"
                  @click="moderate(r.reportId, 'delete')"
                >
                  {{ STR.common.delete }}
                </button>
                <button
                  class="admin__btn admin__btn--ghost"
                  :disabled="r.status !== 'pending'"
                  @click="moderate(r.reportId, 'spoiler')"
                >
                  {{ t.markSpoiler }}
                </button>
              </td>
            </tr>
            <tr v-if="reports.length === 0">
              <td colspan="3" class="admin__empty">{{ t.noReports }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- ═════════ Mode: форма додавання / редагування ═════════ -->
    <template v-else>
      <form class="admin-form" @submit.prevent="submitForm">
        <div class="admin-form__layout">
          <!-- Ліва колонка — інфо про книгу -->
          <div class="admin-form__col">
            <h2 class="admin-form__col-title">{{ t.bookFormSectionTitle }}</h2>

            <label class="admin-form__field">
              <span>{{ t.titleLabel }}</span>
              <input v-model="form.title" type="text" class="admin-form__input" required />
            </label>

            <label class="admin-form__field">
              <span>{{ t.authorLabel }}</span>
              <input v-model="form.author" type="text" class="admin-form__input" />
            </label>

            <label class="admin-form__field">
              <span>{{ t.yearLabel }}</span>
              <input v-model.number="form.year" type="number" class="admin-form__input" min="1900" required />
            </label>

            <label class="admin-form__field">
              <span>{{ t.genreLabel }}</span>
              <select v-model="form.genre" class="admin-form__input">
                <option value="">{{ t.chooseGenre }}</option>
                <option v-for="g in genreOptions" :key="g" :value="g">{{ g }}</option>
              </select>
            </label>

            <!-- BUG-045: тип адаптації (обов'язкове поле бекенду). -->
            <label class="admin-form__field">
              <span>{{ t.typeLabel }}</span>
              <select v-model="form.type" class="admin-form__input" required>
                <option value="movie">{{ t.typeMovie }}</option>
                <option value="series">{{ t.typeSeries }}</option>
                <option value="anime">{{ t.typeAnime }}</option>
              </select>
            </label>

            <label class="admin-form__field">
              <span>{{ t.countryLabel }}</span>
              <input v-model="form.country" type="text" class="admin-form__input" />
            </label>

            <label class="admin-form__field">
              <span>{{ t.posterLabel }}</span>
              <input v-model="form.poster" type="url" class="admin-form__input" placeholder="https://…" />
            </label>

            <div v-if="form.poster" class="admin-form__poster">
              <img :src="form.poster" :alt="t.posterPreviewAlt" @error="onImgError" />
            </div>

            <label class="admin-form__field">
              <span>{{ t.descriptionLabel }}</span>
              <textarea v-model="form.description" rows="4" class="admin-form__input"></textarea>
            </label>

            <div class="admin-form__row">
              <label class="admin-form__field">
                <span>{{ t.bookRatingLabel }}</span>
                <input
                  v-model.number="form.bookRating"
                  type="number"
                  step="0.1"
                  min="0"
                  max="10"
                  class="admin-form__input"
                />
              </label>
              <label class="admin-form__field">
                <span>{{ t.filmRatingLabel }}</span>
                <input
                  v-model.number="form.filmRating"
                  type="number"
                  step="0.1"
                  min="0"
                  max="10"
                  class="admin-form__input"
                />
              </label>
            </div>

            <!-- ── Додаткові адаптації (опціонально: серіал + фільм) ── -->
            <ol v-if="form.extraAdaptations.length" class="admin-form__extras">
              <li v-for="(a, i) in form.extraAdaptations" :key="a.id" class="admin-form__extra">
                <header class="admin-form__extra-head">
                  <span class="admin-form__extra-title">{{ t.extraAdaptationTitle(i + 2) }}</span>
                  <button type="button" class="admin-form__point-remove" @click="removeAdaptation(i)">
                    {{ STR.common.delete }}
                  </button>
                </header>
                <label class="admin-form__field">
                  <span>{{ t.typeLabel }}</span>
                  <select v-model="a.type" class="admin-form__input">
                    <option value="movie">{{ t.typeMovie }}</option>
                    <option value="series">{{ t.typeSeries }}</option>
                    <option value="anime">{{ t.typeAnime }}</option>
                  </select>
                </label>
                <div class="admin-form__row">
                  <label class="admin-form__field">
                    <span>{{ t.yearLabel }}</span>
                    <input v-model.number="a.releaseYear" type="number" min="1900" class="admin-form__input" />
                  </label>
                  <label class="admin-form__field">
                    <span>{{ t.countryLabel }}</span>
                    <input v-model="a.country" type="text" class="admin-form__input" />
                  </label>
                </div>
                <label class="admin-form__field">
                  <span>{{ t.studioLabel }}</span>
                  <input v-model="a.studio" type="text" class="admin-form__input" />
                </label>
                <label class="admin-form__field">
                  <span>{{ t.posterLabel }}</span>
                  <input v-model="a.posterUrl" type="url" class="admin-form__input" placeholder="https://…" />
                </label>
              </li>
            </ol>

            <!-- Кнопка "Додати екранізацію" (під лівою колонкою як у Figma). -->
            <button type="button" class="admin-form__add-adaptation" @click="addAdaptation">
              {{ t.addAdaptation }}
            </button>
          </div>

          <!-- Права колонка — карта відмінностей -->
          <div class="admin-form__col admin-form__col--map">
            <h2 class="admin-form__col-title">{{ t.mapSectionTitle }}</h2>

            <p v-if="form.differences.length === 0" class="admin-form__empty">
              {{ t.noPoints }}
            </p>

            <ol class="admin-form__points">
              <li v-for="(p, i) in form.differences" :key="p.id" class="admin-form__point">
                <header class="admin-form__point-head">
                  <span class="admin-form__point-num">{{ t.point(i + 1) }}</span>
                  <button type="button" class="admin-form__point-remove" @click="removePoint(i)">
                    {{ STR.common.delete }}
                  </button>
                </header>
                <label class="admin-form__field">
                  <span>{{ t.pointTitle }}</span>
                  <input v-model="p.title" type="text" class="admin-form__input" />
                </label>
                <label class="admin-form__field">
                  <span>{{ t.sceneBook }}</span>
                  <textarea v-model="p.bookText" rows="3" class="admin-form__input"></textarea>
                </label>
                <label class="admin-form__field">
                  <span>{{ t.sceneFilm }}</span>
                  <textarea v-model="p.filmText" rows="3" class="admin-form__input"></textarea>
                </label>
                <label class="admin-form__checkbox">
                  <input v-model="p.isSpoiler" type="checkbox" />
                  <span>{{ t.spoiler }}</span>
                </label>
              </li>
            </ol>

            <button type="button" class="admin-form__add-point" @click="addPoint">
              {{ t.addPoint }}
            </button>
          </div>
        </div>

        <div class="admin-form__actions">
          <button type="submit" class="admin__btn admin__btn--primary admin__btn--large" :disabled="isSubmitting">
            {{ isSubmitting ? t.saving : STR.common.confirm }}
          </button>
          <button type="button" class="admin__btn admin__btn--dark admin__btn--large" @click="cancelForm">
            {{ STR.common.cancel }}
          </button>
        </div>
      </form>
    </template>
  </div>
</template>

<style scoped>
.admin {
  display: flex;
  flex-direction: column;
  font-family: var(--font-body);
  padding: 0 0 32px 0;
  min-height: 100%;
}

/* ── Стрічка-заголовок ─────────────────────────────────── */
.admin__stripe {
  background: var(--color-card);
  color: var(--text-on-dark);
  padding: 12px 24px;
  font-family: var(--font-display);
  font-size: 18px;
  margin: 0 0 24px 0;
}

/* ── Layout (Top + Table) ───────────────────────────── */
.admin__top-section {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 80px;
  padding: 0 32px;
  margin-bottom: 40px;
}

.admin__actions-box {
  background: var(--color-panel);
  padding: 32px;
  border-radius: var(--radius-sm);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  gap: 24px;
  height: fit-content;
  width: 420px; /* Збільшена ширина */
  flex-shrink: 0;
  margin-top: 60px; /* Відкориговано, щоб зрівнятися з верхнім краєм картки */
}

.admin__preview-section {
  display: flex;
  flex-direction: column;
  align-items: flex-start; /* Притискає всередині все до лівого краю */
  gap: 24px;
  flex: 1;
  width: 100%;
  max-width: 850px; /* Обгортка для пошуку та картки */
  margin-left: auto; /* Зсуває весь правий блок максимально вправо */
}

.admin__main {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* ── Action buttons ────────────────────────────────────── */
.admin__action {
  background: var(--color-header);
  color: var(--text-on-primary);
  border: none;
  border-radius: 4px;
  padding: 20px 24px;
  font-family: var(--font-body);
  font-size: 20px;
  font-weight: 500;
  cursor: pointer;
  text-align: center;
  box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.4);
  transition: all 0.15s;
  width: 100%;
  box-sizing: border-box;
}

.admin__action:hover {
  opacity: 0.9;
}

/* ── Картка-прев'ю ──────────────────────────────────────── */
.admin__preview-card {
  background: var(--color-panel);
  border-radius: var(--radius-sm);
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  width: 100%;
  max-width: 600px; /* Картка обмежена і притиснута зліва */
}

.admin__preview-content {
  display: flex;
  flex-direction: row;
  gap: 32px;
}

@media (max-width: 500px) {
  .admin__preview-content {
    flex-direction: column;
  }
}

.admin__preview-poster {
  flex: 0 0 250px;
  height: 300px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: transparent;
}

.admin__preview-poster img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.admin__preview-meta {
  flex: 1;
  background: var(--color-primary);
  color: var(--text-on-primary);
  padding: 24px;
  border-radius: var(--radius-xs);
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.admin__preview-meta h3 {
  font-family: var(--font-body);
  font-size: 18px; /* Збільшено шрифт */
  margin: 0 0 16px 0;
  font-weight: 500;
  text-align: center;
}

.admin__preview-meta p {
  margin: 8px 0;
  font-size: 14px; /* Збільшено текст */
  text-align: left;
}

.admin__preview-actions {
  display: flex;
  flex-direction: row;
  gap: 28px; /* Такий самий відступ, як між постером і текстом */
}

.admin__action-wrapper {
  display: flex;
  justify-content: center;
}

.admin__action-wrapper--left {
  flex: 0 0 250px; /* Точно під постером */
}

.admin__action-wrapper--right {
  flex: 1; /* Точно під червоним блоком деталей */
}

.admin__preview-actions .admin__btn {
  width: 140px; /* Акуратна фіксована ширина як на макеті */
  padding: 12px;
  font-size: 16px;
  font-weight: 500;
  border-radius: 4px;
  box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.4);
  background: var(--color-header);
  color: var(--text-on-primary);
}

.admin__preview-actions .admin__btn:hover {
  opacity: 0.9;
}

/* ── Buttons ────────────────────────────────────────────── */
.admin__btn {
  border: none;
  border-radius: var(--radius-sm);
  padding: 6px 14px;
  font-family: var(--font-display);
  font-size: 12px;
  cursor: pointer;
  transition: background 0.15s;
}

.admin__btn--primary {
  background: var(--color-primary);
  color: var(--text-on-primary);
}

.admin__btn--primary:hover:not(:disabled) {
  background: var(--color-primary-hover);
}

.admin__btn--dark {
  background: var(--color-card);
  color: var(--text-on-dark);
}

.admin__btn--dark:hover {
  background: var(--color-primary);
}

.admin__btn--ghost {
  background: var(--color-input-bg);
  color: var(--text-on-light);
  border: 1px solid var(--color-card);
}

.admin__btn--ghost:hover:not(:disabled) {
  background: var(--color-panel-box);
}

.admin__btn--large {
  padding: 10px 32px;
  font-size: 14px;
}

.admin__btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* ── Search ─────────────────────────────────────────────── */
.admin__search {
  position: relative;
  width: 100%; /* Повністю розтягнеться на всі 850px */
}

.admin__search-input {
  width: 100%;
  padding: 10px 44px 10px 16px; /* Ще трохи зменшені відступи */
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  font-family: var(--font-body);
  font-size: 15px; /* Трохи менший шрифт */
  outline: none;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  box-sizing: border-box;
}

.admin__search-input:focus {
  border-color: var(--color-primary);
}

.admin__search-icon {
  position: absolute;
  right: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-on-light);
}

.admin__search-icon svg {
  width: 18px; /* Відповідно трохи менша іконка */
  height: 18px;
}

/* ── Table ──────────────────────────────────────────────── */
.admin__table-section {
  width: 100%;
  overflow-x: auto;
}

.admin__table {
  width: 100%;
  border-collapse: collapse;
  font-family: var(--font-body);
  font-size: 14px;
}

.admin__table th,
.admin__table td {
  padding: 10px 12px;
  text-align: left;
}

.admin__table th {
  font-family: var(--font-display);
  font-weight: 500;
  border-bottom: 2px solid var(--color-card);
  color: var(--text-on-light);
}

.admin__table tbody tr {
  border-bottom: 1px solid var(--color-panel-box);
  cursor: pointer;
  transition: background 0.15s;
}

.admin__table tbody tr:hover {
  background: var(--color-panel-box);
}

.admin__row--selected {
  background: var(--color-panel-box);
}

.admin__row-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.admin__row--approved,
.admin__row--rejected,
.admin__row--marked-spoiler {
  opacity: 0.55;
}

.admin__report-text {
  max-width: 360px;
  word-break: break-word;
}

.admin__empty {
  text-align: center;
  padding: 24px;
  color: var(--text-muted);
}

.admin__status {
  text-align: center;
  padding: 24px;
  color: var(--text-muted);
}

.admin__back {
  align-self: flex-start;
  background: transparent;
  border: 1px solid var(--color-card);
  color: var(--text-on-light);
  padding: 6px 14px;
  border-radius: var(--radius-sm);
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
}

.admin__back:hover {
  background: var(--color-card);
  color: var(--text-on-dark);
}

/* ─────────────── Форма book-form ─────────────── */
.admin-form {
  padding: 0 16px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.admin-form__layout {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 24px;
  align-items: start;
}

.admin-form__col {
  background: var(--color-panel);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: var(--shadow-sm);
}

.admin-form__col-title {
  margin: 0 0 8px 0;
  font-family: var(--font-display);
  font-size: 16px;
  text-align: center;
  font-weight: 500;
  color: var(--text-on-light);
}

.admin-form__field {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-family: var(--font-body);
  font-size: 13px;
}

.admin-form__field span {
  color: var(--text-on-light);
  font-family: var(--font-display);
}

.admin-form__input {
  padding: 8px 12px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  font-family: var(--font-body);
  font-size: 14px;
  color: var(--text-on-light);
  outline: none;
}

.admin-form__input:focus {
  border-color: var(--color-primary);
}

.admin-form__row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.admin-form__poster {
  width: 100%;
  max-width: 200px;
  align-self: center;
  border-radius: var(--radius-sm);
  overflow: hidden;
  border: 1px solid var(--border-default);
}

.admin-form__poster img {
  width: 100%;
  display: block;
}

/* ── Карта точок ────────────────────────────────────────── */
/* Таймлайн точок відмінностей — з кружком (1-ша) і ромбами (інші) ліворуч. */
.admin-form__points {
  list-style: none;
  margin: 0;
  padding: 0 0 0 36px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  position: relative;
}

/* Вертикальна лінія, що з'єднує всі маркери. */
.admin-form__points::before {
  content: '';
  position: absolute;
  left: 11px;
  top: 12px;
  bottom: 12px;
  width: 2px;
  background: var(--color-card);
  z-index: 0;
}

.admin-form__point {
  background: var(--color-panel-box);
  border: 1px solid var(--color-card);
  border-radius: var(--radius-sm);
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  position: relative;
}

/* Маркер ліворуч: 1-ша точка — круг, інші — ромб (квадрат повернутий на 45°). */
.admin-form__point::before {
  content: '';
  position: absolute;
  left: -32px;
  top: 14px;
  width: 14px;
  height: 14px;
  background: var(--color-card);
  /* Ромб за замовчуванням. */
  transform: rotate(45deg);
  z-index: 1;
}

.admin-form__point:first-child::before {
  /* Перша точка — круг. */
  border-radius: 50%;
  transform: none;
  width: 16px;
  height: 16px;
  left: -33px;
  top: 13px;
}

.admin-form__point-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.admin-form__point-num {
  font-family: var(--font-display);
  font-size: 14px;
  color: var(--color-primary);
  font-weight: 600;
}

.admin-form__point-remove {
  background: none;
  border: none;
  color: var(--text-error);
  font-family: var(--font-display);
  font-size: 12px;
  cursor: pointer;
  text-decoration: underline;
}

.admin-form__checkbox {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-body);
  font-size: 13px;
  cursor: pointer;
  color: var(--text-on-light);
}

.admin-form__checkbox input {
  accent-color: var(--color-primary);
}

.admin-form__add-point {
  align-self: center;
  background: var(--color-card);
  color: var(--text-on-dark);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 8px 24px;
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
}

.admin-form__add-point:hover {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

/* ── Додаткові адаптації (екранізації) ────────────────── */
.admin-form__extras {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.admin-form__extra {
  background: var(--color-panel-box);
  border: 1px solid var(--color-card);
  border-radius: var(--radius-sm);
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.admin-form__extra-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.admin-form__extra-title {
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 600;
  color: var(--color-primary);
}

/* Кнопка "+ Додати екранізацію" — темно-винна, на всю ширину колонки. */
.admin-form__add-adaptation {
  width: 100%;
  background: var(--color-card);
  color: var(--text-on-dark);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 14px 20px;
  font-family: var(--font-display);
  font-size: 15px;
  cursor: pointer;
  margin-top: 8px;
  transition: background 0.15s;
}

.admin-form__add-adaptation:hover {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.admin-form__empty {
  text-align: center;
  font-family: var(--font-body);
  font-size: 13px;
  color: var(--text-muted);
}

.admin-form__actions {
  display: flex;
  gap: 16px;
  justify-content: center;
}

@media (max-width: 900px) {
  .admin__layout {
    grid-template-columns: 1fr;
  }
  .admin-form__layout {
    grid-template-columns: 1fr;
  }
}
</style>
