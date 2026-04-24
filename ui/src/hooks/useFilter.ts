import { computed, type Ref } from 'vue';
import type { BookScreenItem } from '../services/types';

// Composable для клієнтської фільтрації списку творів.
// Приймає `items` як Ref, тому коректно оновлюється після
// асинхронного завантаження з API.

export function useFilter(
  items: Ref<BookScreenItem[]> | BookScreenItem[],
  searchQuery: Ref<string>,
  selectedGenre: Ref<string | null>,
  selectedCountry: Ref<string | null>
) {
  const filteredItems = computed(() => {
    const list = Array.isArray(items) ? items : items.value;
    const query = searchQuery.value.toLowerCase().trim();

    return list.filter((item) => {
      const matchesSearch =
        !query ||
        item.title.toLowerCase().includes(query) ||
        item.genre.toLowerCase().includes(query) ||
        item.country.toLowerCase().includes(query) ||
        String(item.year).includes(query) ||
        item.description.toLowerCase().includes(query);

      const matchesGenre = !selectedGenre.value || item.genre.toLowerCase().includes(selectedGenre.value.toLowerCase());

      const matchesCountry = !selectedCountry.value || item.country === selectedCountry.value;

      return matchesSearch && matchesGenre && matchesCountry;
    });
  });

  return { filteredItems };
}
