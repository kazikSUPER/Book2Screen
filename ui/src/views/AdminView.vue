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
  title: string;
  author: string;
  releaseYear: number | null;
  genre: string;
  posterUrl: string;
  description: string;
  type: string;
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

// Додає блок ще однієї екранізації під формою.
function addAdaptation(): void {
  form.value.extraAdaptations.push({
    id: `adapt-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
    title: '',
    author: '',
    releaseYear: null,
    genre: '',
    posterUrl: '',
    description: '',
    type: 'movie',
  });
}

function removeAdaptation(index: number): void {
  form.value.extraAdaptations.splice(index, 1);
}

const form = ref<BookForm>(emptyForm());
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

function checkLeaveForm(): boolean {
  if (mode.value !== 'book-form') return true;
  const isDirty =
    form.value.title.trim() !== '' ||
    form.value.author.trim() !== '' ||
    form.value.year !== null ||
    form.value.genre !== '' ||
    form.value.country.trim() !== '' ||
    form.value.poster.trim() !== '' ||
    form.value.description.trim() !== '' ||
    form.value.differences.length > 0 ||
    form.value.extraAdaptations.length > 0;

  if (isDirty && !confirm(t.confirmCancelForm)) return false;
  return true;
}

// ── Mode switchers ──────────────────────────────────────────
function switchToBooks(): void {
  if (!checkLeaveForm()) return;
  mode.value = 'books';
}

function switchToComments(): void {
  if (!checkLeaveForm()) return;
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
  if (!checkLeaveForm()) return;
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
async function moderate(reportId: string, action: 'approve' | 'reject' | 'spoiler'): Promise<void> {
  try {
    await moderateReport(reportId, action);
    // BUG-041: одразу прибираємо запис зі списку, щоб UI оновився
    // (раніше тільки міняли status, а кнопка лишалась видимою з disabled).
    reports.value = reports.value.filter((x) => x.reportId !== reportId);
    const labels = {
      approve: t.commentDeleted,
      reject: t.reportRejected,
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
                  @click="moderate(r.reportId, 'reject')"
                >
                  {{ t.approve }}
                </button>
                <button
                  class="admin__btn admin__btn--primary"
                  :disabled="r.status !== 'pending'"
                  @click="moderate(r.reportId, 'approve')"
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
          <!-- ═══ Ліва колонка — інфо про книгу ═══ -->
          <div class="admin-form__col">
            <h2 class="admin-form__col-title">{{ t.bookFormSectionTitle }}</h2>

            <!-- BUG-045: type обов'язковий для бекенду -->
            <input v-model="form.type" type="hidden" />

            <input v-model="form.title" type="text" class="admin-form__input" :placeholder="t.titleLabel" required />
            <input v-model="form.author" type="text" class="admin-form__input" :placeholder="t.authorLabel" />
            <input
              v-model.number="form.year"
              type="number"
              class="admin-form__input"
              :placeholder="t.yearLabel"
              min="1900"
              required
            />

            <select v-model="form.genre" class="admin-form__input admin-form__select">
              <option value="">{{ t.chooseGenre }}</option>
              <option v-for="g in genreOptions" :key="g" :value="g">{{ g }}</option>
            </select>

            <!-- Постер -->
            <div class="admin-form__poster-section">
              <label class="admin-form__poster-link" for="poster-url-input">
                {{ t.posterLabel }}
              </label>
              <input
                id="poster-url-input"
                v-model="form.poster"
                type="url"
                class="admin-form__poster-url"
                placeholder="https://…"
              />
              <div class="admin-form__poster-preview">
                <img v-if="form.poster" :src="form.poster" :alt="t.posterPreviewAlt" @error="onImgError" />
              </div>
            </div>

            <textarea
              v-model="form.description"
              rows="4"
              class="admin-form__input admin-form__textarea"
              :placeholder="t.descriptionLabel"
            ></textarea>

            <button type="button" class="admin-form__add-adaptation" @click="addAdaptation">
              {{ t.addAdaptation }}
            </button>

            <!-- ── Секція додаткових екранізацій ── -->
            <div v-if="form.extraAdaptations.length" class="admin-form__adaptations">
              <ol class="admin-form__extras">
                <li v-for="(a, i) in form.extraAdaptations" :key="a.id" class="admin-form__extra">
                  <header class="admin-form__extra-head">
                    <span class="admin-form__extra-title">{{ t.extraAdaptationTitle(i + 2) }}</span>
                    <button type="button" class="admin-form__point-remove" @click="removeAdaptation(i)">
                      {{ STR.common.delete }}
                    </button>
                  </header>

                  <div class="admin-form__extra-fields">
                    <input
                      v-model="a.title"
                      type="text"
                      class="admin-form__input"
                      :placeholder="t.titleLabel"
                      required
                    />
                    <input v-model="a.author" type="text" class="admin-form__input" :placeholder="t.authorLabel" />
                    <input
                      v-model.number="a.releaseYear"
                      type="number"
                      class="admin-form__input"
                      :placeholder="t.yearLabel"
                      min="1900"
                      required
                    />

                    <select v-model="a.genre" class="admin-form__input admin-form__select">
                      <option value="">{{ t.chooseGenre }}</option>
                      <option v-for="g in genreOptions" :key="g" :value="g">{{ g }}</option>
                    </select>

                    <div class="admin-form__poster-section">
                      <label class="admin-form__poster-link" :for="'poster-' + a.id">
                        {{ t.posterLabel }}
                      </label>
                      <input
                        :id="'poster-' + a.id"
                        v-model="a.posterUrl"
                        type="url"
                        class="admin-form__poster-url"
                        placeholder="https://…"
                      />
                      <div class="admin-form__poster-preview">
                        <img v-if="a.posterUrl" :src="a.posterUrl" :alt="t.posterPreviewAlt" @error="onImgError" />
                      </div>
                    </div>

                    <textarea
                      v-model="a.description"
                      rows="4"
                      class="admin-form__input admin-form__textarea"
                      :placeholder="t.descriptionLabel"
                    ></textarea>
                  </div>
                </li>
              </ol>
            </div>
          </div>

          <!-- ═══ Права колонка — карта відмінностей ═══ -->
          <div class="admin-form__col admin-form__col--map">
            <h2 class="admin-form__col-title">{{ t.mapSectionTitle }}</h2>

            <p v-if="form.differences.length === 0" class="admin-form__empty">
              {{ t.noPoints }}
            </p>

            <ol class="admin-form__points">
              <li v-for="(p, i) in form.differences" :key="p.id" class="admin-form__point">
                <header class="admin-form__point-head">
                  <div class="admin-form__point-labels">
                    <span class="admin-form__point-num">Точка {{ i + 1 }}</span>
                    <span class="admin-form__card-label">{{ t.pointTitle }}</span>
                  </div>
                  <button type="button" class="admin-form__point-remove" @click="removePoint(i)">
                    {{ STR.common.delete }}
                  </button>
                </header>
                <input v-model="p.title" type="text" class="admin-form__card-input" />

                <span class="admin-form__card-label">{{ t.sceneBook }}</span>
                <textarea
                  v-model="p.bookText"
                  rows="3"
                  class="admin-form__card-input admin-form__card-textarea"
                ></textarea>

                <span class="admin-form__card-label">{{ t.sceneFilm }}</span>
                <textarea
                  v-model="p.filmText"
                  rows="3"
                  class="admin-form__card-input admin-form__card-textarea"
                ></textarea>

                <label class="admin-form__checkbox">
                  <input v-model="p.isSpoiler" type="checkbox" />
                  <span>{{ t.spoiler }}</span>
                </label>
              </li>
            </ol>

            <button type="button" class="admin-form__add-point" @click="addPoint">
              {{ t.addPoint }}
            </button>

            <div class="admin-form__actions">
              <button type="submit" class="admin-form__btn-confirm" :disabled="isSubmitting">
                {{ isSubmitting ? t.saving : STR.common.confirm }}
              </button>
              <button type="button" class="admin-form__btn-cancel" @click="cancelForm">
                {{ STR.common.cancel }}
              </button>
            </div>
          </div>
        </div>
      </form>
    </template>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Comfortaa:wght@400;500;600;700&display=swap');

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
  grid-template-columns: 340px 1fr;
}

.admin-form__col {
  border: 1px solid #000;
  border-radius: 0;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  box-shadow: none;
}

.admin-form__col:first-child {
  background: #d19f9f;
  border-right: none;
}

.admin-form__col--map {
  background: #f7cccc;
}

/* ── Назва колонки ──────────────────────────────────────── */
.admin-form__col-title {
  margin: 0 0 10px 0;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 18px;
  text-align: center;
  font-weight: 700;
  color: #000;
  letter-spacing: normal;
}

/* ── Інпути лівої колонки (плейсхолдери замість лейблів) ─ */
.admin-form__input {
  width: 100%;
  box-sizing: border-box;
  padding: 10px 12px;
  border: 1px solid #000;
  border-radius: 0;
  background: transparent;
  font-family: 'Comfortaa', var(--font-body), sans-serif;
  font-size: 14px;
  color: #000;
  outline: none;
  transition:
    border-color 0.15s,
    box-shadow 0.15s;
}

.admin-form__input:focus {
  border-color: #000;
  box-shadow: 0 0 0 1px #000;
}

.admin-form__input::placeholder {
  color: #000;
}

.admin-form__select {
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23000' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'%3E%3C/polyline%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 12px center;
  padding-right: 32px;
}

.admin-form__textarea {
  resize: vertical;
  min-height: 80px;
}

/* ── Секція постера ─────────────────────────────────────── */
.admin-form__poster-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
  border: 1px solid #000;
  padding: 8px;
}

.admin-form__poster-link {
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 14px;
  color: #0000ee;
  text-decoration: underline;
  cursor: pointer;
  width: 100%;
  text-align: left;
  margin-left: 4px;
}

.admin-form__poster-url {
  width: 100%;
  box-sizing: border-box;
  padding: 6px 10px;
  border: none;
  border-bottom: 1px solid #000;
  border-radius: 0;
  background: transparent;
  font-family: var(--font-body);
  font-size: 13px;
  color: #000;
  outline: none;
}

.admin-form__poster-url:focus {
  border-color: #000;
}

.admin-form__poster-preview {
  width: 100%;
  min-height: 150px;
  background: transparent;
  border: none;
  border-radius: 0;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.admin-form__poster-preview img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}

/* ── Кнопка "Додати екранізацію" (в лівій колонці) ──────── */
.admin-form__add-adaptation {
  width: 100%;
  background: #3b1414;
  color: #fff;
  border: none;
  border-radius: 0;
  padding: 14px 16px;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  margin-top: 4px;
  box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.4);
  transition:
    background 0.15s,
    box-shadow 0.15s;
}

.admin-form__add-adaptation:hover {
  background: #5c2e2e;
  box-shadow: 0 5px 14px rgba(0, 0, 0, 0.3);
}

/* ── Таймлайн правої колонки ────────────────────────────── */
.admin-form__points {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 20px;
  position: relative;
}

/* Вертикальна лінія */
.admin-form__points::before {
  content: '';
  position: absolute;
  left: calc(25% - 30px);
  top: 16px;
  bottom: 16px;
  width: 1px;
  background: #000;
  z-index: 0;
}

.admin-form__point {
  background: transparent;
  border: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  position: relative;
}

/* Маркер — ромб за замовчуванням */
.admin-form__point::before {
  content: '';
  position: absolute;
  left: calc(25% - 38px);
  top: 14px;
  width: 16px;
  height: 16px;
  background: #000;
  transform: rotate(45deg);
  z-index: 1;
}

/* Перша точка — круг */
.admin-form__point:first-child::before {
  border-radius: 50%;
  transform: none;
  width: 18px;
  height: 18px;
  left: calc(25% - 39px);
  top: 13px;
}

.admin-form__point-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2px;
  width: 50%;
  align-self: center;
}

.admin-form__point-labels {
  display: flex;
  align-items: center;
  gap: 12px;
}

.admin-form__point-num {
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 15px;
  color: #000;
  font-weight: 500;
}

.admin-form__point-remove {
  background: none;
  border: none;
  color: #c0392b;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 12px;
  cursor: pointer;
  text-decoration: underline;
}

/* ── Карточки полів у правій колонці ────────────────────── */
.admin-form__card-label {
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 14px;
  color: #000;
  font-weight: 500;
  margin-top: 2px;
  width: 50%;
  align-self: center;
}

.admin-form__card-input {
  width: 50%;
  align-self: center;
  box-sizing: border-box;
  padding: 15px 14px;
  border: 1px solid #000;
  border-radius: 0;
  background: #bb8d8d;
  color: #000;
  font-family: 'Comfortaa', var(--font-body), sans-serif;
  font-size: 14px;
  outline: none;
  box-shadow: 3px 3px 6px rgba(0, 0, 0, 0.2);
  transition: box-shadow 0.15s;
}

.admin-form__card-input:focus {
  box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.3);
}

.admin-form__card-input::placeholder {
  color: #7a5555;
}

.admin-form__card-textarea {
  resize: vertical;
  min-height: 100px;
}

/* ── Чекбокс "Спойлер" ─────────────────────────────────── */
.admin-form__checkbox {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  font-family: 'Comfortaa', var(--font-body), sans-serif;
  font-size: 16px;
  cursor: pointer;
  color: #000;
  margin-top: 4px;
  align-self: center;
}

.admin-form__checkbox input {
  appearance: none;
  width: 24px;
  height: 24px;
  border: 2px solid #000;
  border-radius: 0;
  background-color: transparent;
  cursor: pointer;
  position: relative;
}

.admin-form__checkbox input:checked::after {
  content: '';
  position: absolute;
  left: 6px;
  top: 2px;
  width: 6px;
  height: 12px;
  border: solid #000;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

/* ── Кнопка "+ Подати нову точку" ───────────────────────── */
.admin-form__add-point {
  width: 100%;
  background: #2e1319;
  color: #fff;
  border: none;
  border-radius: 0;
  padding: 14px 20px;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  box-shadow: 2px 2px 6px rgba(0, 0, 0, 0.4);
  transition:
    background 0.15s,
    box-shadow 0.15s;
  max-width: 300px;
  margin: 0 auto;
}

.admin-form__add-point:hover {
  background: #471d26;
  box-shadow: 3px 3px 8px rgba(0, 0, 0, 0.5);
}

/* ── Кнопки "Підтвердити" / "Скасувати" ─────────────────── */
.admin-form__actions {
  display: flex;
  gap: 60px;
  justify-content: center;
  margin-top: 24px;
}

.admin-form__btn-confirm {
  background: #8c243a;
  color: #fff;
  border: none;
  border-radius: 0;
  padding: 12px 40px;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.3);
  transition:
    background 0.15s,
    box-shadow 0.15s;
}

.admin-form__btn-confirm:hover:not(:disabled) {
  background: #a92b46;
  box-shadow: 3px 3px 8px rgba(0, 0, 0, 0.4);
}

.admin-form__btn-confirm:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.admin-form__btn-cancel {
  background: #3b1111;
  color: #fff;
  border: none;
  border-radius: 0;
  padding: 12px 40px;
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.3);
  transition: background 0.15s;
}

.admin-form__btn-cancel:hover {
  background: #531818;
}

/* ── Секція додаткових екранізацій ──────────────────────── */
.admin-form__adaptations {
  padding: 0;
  margin-top: 4px;
}

.admin-form__extras {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.admin-form__extra {
  background: transparent;
  border: 1px solid #000;
  border-radius: 0;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: none;
}

.admin-form__extra-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.admin-form__extra-title {
  font-family: 'Comfortaa', var(--font-display), sans-serif;
  font-size: 15px;
  font-weight: 700;
  color: #000;
}

.admin-form__extra-grid {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.admin-form__extra-fields {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.admin-form__empty {
  text-align: center;
  font-family: 'Comfortaa', var(--font-body), sans-serif;
  font-size: 13px;
  color: var(--text-muted);
}

@media (max-width: 700px) {
  .admin-form__layout {
    grid-template-columns: 1fr;
  }
  .admin-form__extra-grid {
    grid-template-columns: 1fr;
  }
  .admin-form__extra-fields {
    grid-template-columns: 1fr;
  }
}
</style>
