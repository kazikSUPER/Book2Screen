import { computed, type Ref } from 'vue';
import type { BookScreenItem } from '../services/types';
import { useFiltersStore, type SortOption } from '../state/filters';
import { mapGenreToBackend, mapCountryToBackend } from '../constants/catalog';

export function useFilter(items: Ref<BookScreenItem[]> | BookScreenItem[]) {
  const filters = useFiltersStore();

  const filteredItems = computed(() => {
    const list = Array.isArray(items) ? items : items.value;
    const query = filters.searchQuery.toLowerCase().trim();

    const matched = list.filter((item) => {
      const matchesSearch =
        !query ||
        item.title.toLowerCase().includes(query) ||
        item.genre.toLowerCase().includes(query) ||
        item.country.toLowerCase().includes(query) ||
        String(item.year).includes(query) ||
        item.description.toLowerCase().includes(query);

      const genreEn = mapGenreToBackend(filters.genre);
      const matchesGenre =
        !filters.genre ||
        item.genre.toLowerCase().includes((genreEn ?? '').toLowerCase()) ||
        item.genre.toLowerCase().includes(filters.genre.toLowerCase());

      const countryEn = mapCountryToBackend(filters.country);
      const matchesCountry = !filters.country || item.country === countryEn || item.country === filters.country;

      const matchesYearMin = filters.yearMin === null || item.year >= filters.yearMin;
      const matchesYearMax = filters.yearMax === null || item.year <= filters.yearMax;

      const matchesRating =
        filters.minRating === null || Math.max(item.bookRating, item.filmRating) >= filters.minRating;

      const matchesMap = !filters.onlyWithMap || item.hasMap === true;

      return (
        matchesSearch &&
        matchesGenre &&
        matchesCountry &&
        matchesYearMin &&
        matchesYearMax &&
        matchesRating &&
        matchesMap
      );
    });

    return sortItems(matched, filters.sortBy);
  });

  return { filteredItems };
}

function sortItems(list: BookScreenItem[], sort: SortOption): BookScreenItem[] {
  const sorted = [...list];
  switch (sort) {
    case 'rating-desc':
      return sorted.sort((a, b) => Math.max(b.bookRating, b.filmRating) - Math.max(a.bookRating, a.filmRating));
    case 'year-desc':
      return sorted.sort((a, b) => b.year - a.year);
    case 'year-asc':
      return sorted.sort((a, b) => a.year - b.year);
    case 'popular':
    default:
      return sorted;
  }
}
