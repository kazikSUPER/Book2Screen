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
});

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
  if (!form.value.title || !form.value.year) {
    notifications.pushWarning(t.fillTitleAndYear);
    return;
  }
  isSubmitting.value = true;
  try {
    const payload: Omit<BookScreenItem, 'id'> = {
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

function cancelForm(): void {
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

// ── Comment moderation actions ──────────────────────────────
async function moderate(reportId: string, action: 'approve' | 'reject' | 'spoiler'): Promise<void> {
  try {
    await moderateReport(reportId, action);
    const r = reports.value.find((x) => x.reportId === reportId);
    if (r) {
      r.status = action === 'approve' ? 'approved' : action === 'reject' ? 'rejected' : 'marked-spoiler';
    }
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
      <h1 v-if="mode === 'books'" class="admin__stripe-title">{{ t.panelTitle }}</h1>
      <h1 v-else-if="mode === 'comments'" class="admin__stripe-title">{{ t.moderationTitle }}</h1>
      <h1 v-else class="admin__stripe-title">{{ form.id ? t.editBookTitle : t.addBookTitle }}</h1>
    </div>

    <!-- ═════════ Mode: книги ═════════ -->
    <template v-if="mode === 'books'">
      <div class="admin__layout">
        <!-- Sidebar з діями -->
        <aside class="admin__sidebar">
          <button class="admin__action" @click="startCreate">{{ t.addBook }}</button>
          <button class="admin__action admin__action--secondary" @click="switchToComments">
            {{ t.moderation }}
          </button>

          <!-- Картка-прев'ю обраної книги -->
          <article v-if="selectedBook" class="admin__preview">
            <div class="admin__preview-poster">
              <img :src="selectedBook.poster" :alt="selectedBook.title" />
            </div>
            <div class="admin__preview-meta">
              <h3>{{ selectedBook.title }}</h3>
              <p><strong>Рік:</strong> {{ selectedBook.year }}</p>
              <p><strong>Жанр:</strong> {{ selectedBook.genre }}</p>
              <p><strong>Країна:</strong> {{ selectedBook.country }}</p>
              <p v-if="selectedBook.author"><strong>Автор:</strong> {{ selectedBook.author }}</p>
              <p><strong>Рейтинг:</strong> {{ selectedBook.bookRating }} / 10</p>
            </div>
            <div class="admin__preview-actions">
              <button class="admin__btn admin__btn--primary" @click="startEdit(selectedBook)">
                {{ STR.common.edit }}
              </button>
              <button class="admin__btn admin__btn--dark" @click="onDelete(selectedBook)">
                {{ STR.common.delete }}
              </button>
            </div>
          </article>
        </aside>

        <!-- Main: пошук + таблиця -->
        <section class="admin__main">
          <div class="admin__search">
            <input v-model="searchQuery" type="text" :placeholder="t.searchPlaceholder" class="admin__search-input" />
            <span class="admin__search-icon" aria-hidden="true">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
                <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" />
                <path d="M21 21L16.5 16.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              </svg>
            </span>
          </div>

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
                  <button class="admin__btn admin__btn--primary" @click="onDelete(b)">{{ STR.common.delete }}</button>
                </td>
              </tr>
              <tr v-if="filteredBooks.length === 0">
                <td colspan="5" class="admin__empty">{{ t.nothingFound }}</td>
              </tr>
            </tbody>
          </table>
        </section>
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

            <label class="admin-form__field">
              <span>{{ t.countryLabel }}</span>
              <input v-model="form.country" type="text" class="admin-form__input" />
            </label>

            <label class="admin-form__field">
              <span>{{ t.posterLabel }}</span>
              <input v-model="form.poster" type="url" class="admin-form__input" placeholder="https://…" />
            </label>

            <div v-if="form.poster" class="admin-form__poster">
              <img :src="form.poster" :alt="t.posterPreviewAlt" />
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
  margin: -16px -16px 24px -16px;
}

.admin__stripe-title {
  font-size: inherit;
  font-weight: inherit;
  margin: 0;
  display: inline;
}

/* ── Layout (sidebar + main) ───────────────────────────── */
.admin__layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 24px;
  padding: 0 16px;
}

.admin__sidebar {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.admin__main {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 0 16px;
}

/* ── Sidebar buttons ────────────────────────────────────── */
.admin__action {
  background: var(--color-card);
  color: var(--text-on-dark);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 14px 16px;
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  text-align: center;
  box-shadow: var(--shadow-sm);
  transition: all 0.15s;
}

.admin__action:hover {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.admin__action--secondary {
  background: var(--color-card);
}

/* ── Картка-прев'ю ──────────────────────────────────────── */
.admin__preview {
  background: var(--color-card);
  color: var(--text-on-dark);
  border-radius: var(--radius-md);
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  box-shadow: var(--shadow-sm);
}

.admin__preview-poster {
  width: 100%;
  height: 220px;
  overflow: hidden;
  border-radius: var(--radius-xs);
  background: var(--color-header);
}

.admin__preview-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.admin__preview-meta h3 {
  font-family: var(--font-body);
  font-size: 14px;
  margin: 0 0 6px 0;
  font-weight: 600;
}

.admin__preview-meta p {
  margin: 2px 0;
  font-size: 12px;
}

.admin__preview-actions {
  display: flex;
  gap: 8px;
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
  max-width: 600px;
}

.admin__search-input {
  width: 100%;
  padding: 9px 36px 9px 14px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  font-family: var(--font-body);
  font-size: 14px;
  outline: none;
}

.admin__search-input:focus {
  border-color: var(--color-primary);
}

.admin__search-icon {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-on-light);
}

/* ── Table ──────────────────────────────────────────────── */
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
.admin-form__points {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.admin-form__point {
  background: var(--color-panel-box);
  border: 1px solid var(--color-card);
  border-radius: var(--radius-sm);
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
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
