/**
 * Каталожні довідники: жанри, країни, варіанти сортування, причини скарги.
 *
 * До цього вони дублювалися у FilterPanel.vue, TopFilterBar.vue,
 * AdminView.vue, ReportCommentModal.vue. Тепер — одне джерело правди.
 *
 * Коли бекенд додасть GET /api/v1/genres, GET /api/v1/countries — підмінимо
 * на запит з кешем. Поки що — статичний список, узгоджений з Figma-макетом.
 */

import type { SortOption } from '../state/filters';

export const GENRES: readonly string[] = [
  'Комедія',
  'Драма',
  'Фантастика',
  'Фентезі',
  'Жахи',
  'Детектив',
  'Кримінал',
  'Пригоди',
  'Історичні',
  'Біографічні',
  'Документальні',
] as const;

export const COUNTRIES: readonly string[] = [
  'Україна',
  'США',
  'Велика Британія',
  'Канада',
  'Греція',
  'Італія',
  'Туреччина',
  'Іспанія',
  'Німеччина',
  'Японія',
  'Швеція',
] as const;

export interface SortOptionItem {
  value: SortOption;
  label: string;
}

// Варіанти для вертикальної FilterPanel (Home/Search).
export const SORT_OPTIONS_VERTICAL: readonly SortOptionItem[] = [
  { value: 'popular', label: 'Популярні' },
  { value: 'rating-desc', label: 'Рейтинг ↓' },
  { value: 'year-desc', label: 'Спочатку нові' },
  { value: 'year-asc', label: 'Спочатку старі' },
] as const;

// Варіанти для горизонтальної TopFilterBar — формулювання інше за Figma.
export const SORT_OPTIONS_TOP: readonly SortOptionItem[] = [
  { value: 'popular', label: 'За популярністю' },
  { value: 'rating-desc', label: 'За рейтингом' },
  { value: 'year-desc', label: 'Спочатку нові' },
  { value: 'year-asc', label: 'Спочатку старі' },
] as const;

export const REPORT_REASONS: readonly string[] = [
  'Спойлер без позначки',
  'Образи / нецензурна лексика',
  'Спам або реклама',
  'Не по темі',
  'Інше',
] as const;

// Межі для контролу року у фільтрах.
export const MIN_YEAR = 1900;
export const MAX_YEAR = new Date().getFullYear();

/**
 * BUG-042: бек зберігає жанри/країни англійською (Sci-Fi, USA),
 * а UI показує українські назви (Фантастика, США).
 * Мапа конвертує UI-значення у те, що чекає бек у query params.
 *
 * Коли бек віддасть локалізовані словники з API — мапу можна буде видалити.
 */
export const GENRE_UA_TO_BE: Readonly<Record<string, string>> = {
  Комедія: 'Comedy',
  Драма: 'Drama',
  Фантастика: 'Sci-Fi',
  Фентезі: 'Fantasy',
  Жахи: 'Horror',
  Детектив: 'Detective',
  Кримінал: 'Crime',
  Пригоди: 'Adventure',
  Історичні: 'Historical',
  Біографічні: 'Biography',
  Документальні: 'Documentary',
};

export const COUNTRY_UA_TO_BE: Readonly<Record<string, string>> = {
  Україна: 'Ukraine',
  США: 'USA',
  'Велика Британія': 'UK',
  Канада: 'Canada',
  Греція: 'Greece',
  Італія: 'Italy',
  Туреччина: 'Turkey',
  Іспанія: 'Spain',
  Німеччина: 'Germany',
  Японія: 'Japan',
  Швеція: 'Sweden',
};

/** Якщо нема перекладу — повертаємо як є (пошук теж може приймати укр). */
export function mapGenreToBackend(uaValue: string | null | undefined): string | null {
  if (!uaValue) return null;
  return GENRE_UA_TO_BE[uaValue] ?? uaValue;
}

export function mapCountryToBackend(uaValue: string | null | undefined): string | null {
  if (!uaValue) return null;
  return COUNTRY_UA_TO_BE[uaValue] ?? uaValue;
}
