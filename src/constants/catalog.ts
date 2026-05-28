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
