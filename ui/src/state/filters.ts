import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';

/**
 * Глобальний стан фільтрів каталогу.
 *
 * Одне джерело правди для Home, Search, Top, Catalog. FilterPanel
 * (вертикальна) і TopFilterBar (горизонтальна) пишуть сюди ж.
 *
 * Persistence (localStorage) — щоб користувач після перезавантаження
 * сторінки бачив свої фільтри застосованими.
 */

export type SortOption = 'popular' | 'rating-desc' | 'year-desc' | 'year-asc';

export const useFiltersStore = defineStore('filters', () => {
  const searchQuery = usePersistedRef<string>('b2s_filter_search', '');
  const genre = usePersistedRef<string | null>('b2s_filter_genre', null);
  const country = usePersistedRef<string | null>('b2s_filter_country', null);

  // Рік: діапазон [min, max]. null = без обмеження.
  const yearMin = usePersistedRef<number | null>('b2s_filter_year_min', null);
  const yearMax = usePersistedRef<number | null>('b2s_filter_year_max', null);

  // Мінімальний рейтинг (0..10). null = без обмеження.
  const minRating = usePersistedRef<number | null>('b2s_filter_min_rating', null);

  const sortBy = usePersistedRef<SortOption>('b2s_filter_sort', 'popular');

  // SCRUM-67: чекбокс "Лише з картою відмінностей" у TopView.
  const onlyWithMap = usePersistedRef<boolean>('b2s_filter_only_with_map', false);

  const hasActiveFilters = computed(() => {
    return (
      searchQuery.value.trim() !== '' ||
      genre.value !== null ||
      country.value !== null ||
      yearMin.value !== null ||
      yearMax.value !== null ||
      minRating.value !== null ||
      sortBy.value !== 'popular' ||
      onlyWithMap.value
    );
  });

  function clearAll(): void {
    searchQuery.value = '';
    genre.value = null;
    country.value = null;
    yearMin.value = null;
    yearMax.value = null;
    minRating.value = null;
    sortBy.value = 'popular';
    onlyWithMap.value = false;
  }

  return {
    searchQuery,
    genre,
    country,
    yearMin,
    yearMax,
    minRating,
    sortBy,
    onlyWithMap,
    hasActiveFilters,
    clearAll,
  };
});
