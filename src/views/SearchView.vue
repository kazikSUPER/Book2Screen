<script lang="ts">
import { defineComponent, ref, computed, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useFilter } from '../hooks/useFilter';
import type { BookScreenItem } from '../services/types';
import { fetchWorks } from '../services/works';
import { extractErrorMessage } from '../services/error';
import { useFiltersStore } from '../state/filters';

export default defineComponent({
  name: 'SearchView',
  setup() {
    const route = useRoute();
    const router = useRouter();
    const filters = useFiltersStore();

    const items = ref<BookScreenItem[]>([]);
    const isLoading = ref(false);
    const errorMessage = ref('');

    const { filteredItems } = useFilter(items);

    // Якщо у URL прийшов ?q= (наприклад, після Enter у пошуку з шапки) —
    // синхронізуємо його зі стором фільтрів, щоб панель і сторінка показували те ж саме.
    if (route.query.q) {
      filters.searchQuery = route.query.q as string;
    }

    const loadItems = async (): Promise<void> => {
      isLoading.value = true;
      errorMessage.value = '';
      try {
        // Шлемо search-параметр одразу в API; локальна фільтрація — як fallback,
        // якщо backend не вміє фільтрувати або повертає більше ніж потрібно.
        items.value = await fetchWorks({ search: filters.searchQuery || null });
      } catch (err) {
        errorMessage.value = extractErrorMessage(err);
        items.value = [];
      } finally {
        isLoading.value = false;
      }
    };

    onMounted(loadItems);

    // URL змінився (новий пошук з шапки) → оновити query і перезавантажити.
    watch(
      () => route.query.q,
      (newQuery) => {
        filters.searchQuery = (newQuery as string) || '';
        loadItems();
      }
    );

    const resultsCount = computed(() => filteredItems.value.length);

    const goToItem = (id: string): void => {
      router.push({ name: 'detail', params: { id } });
    };

    return {
      searchQuery: computed(() => filters.searchQuery),
      filteredItems,
      isLoading,
      errorMessage,
      resultsCount,
      loadItems,
      goToItem,
    };
  },
});
</script>

<template>
  <div class="search-page">
    <h1 class="page-title">Результати пошуку</h1>

    <p v-if="searchQuery" class="search-info">
      Запит: <strong>"{{ searchQuery }}"</strong>
      <span v-if="!isLoading"> · Знайдено: {{ resultsCount }}</span>
    </p>

    <p v-if="isLoading" class="status">Завантаження...</p>

    <div v-else-if="errorMessage" class="status">
      <p>⚠ {{ errorMessage }}</p>
      <button class="retry-btn" @click="loadItems">Повторити</button>
    </div>

    <p v-else-if="filteredItems.length === 0" class="status">Нічого не знайдено 😔</p>

    <div v-else class="results-grid">
      <div v-for="item in filteredItems" :key="item.id" class="result-card" @click="goToItem(item.id)">
        <img :src="item.poster" :alt="item.title" class="result-poster" />
        <div class="result-info">
          <h3 class="result-title">{{ item.title }}</h3>
          <p class="result-meta">{{ item.year }} · {{ item.genre }} · {{ item.country }}</p>
          <p class="result-desc">{{ item.description }}</p>
          <div class="result-ratings">
            <span class="rating book">📖 {{ item.bookRating }}</span>
            <span class="rating film">🎬 {{ item.filmRating }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.search-page {
  padding: 24px;
  font-family: 'Georgia', serif;
}

.page-title {
  font-size: 28px;
  color: var(--dark-card);
  margin: 0 0 8px;
}

.search-info {
  color: var(--accent);
  font-size: 16px;
  margin: 0 0 24px;
}

.status {
  text-align: center;
  padding: 60px 0;
  color: var(--accent);
  font-size: 16px;
}

.retry-btn {
  margin-top: 12px;
  padding: 10px 24px;
  background: var(--accent);
  color: var(--pink-light);
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.results-grid {
  display: grid;
  gap: 16px;
}

.result-card {
  display: flex;
  gap: 16px;
  background: var(--dark-card);
  border-radius: 12px;
  padding: 16px;
  cursor: pointer;
  transition: transform 0.15s;
}

.result-card:hover {
  transform: translateY(-2px);
}

.result-poster {
  width: 120px;
  height: 180px;
  object-fit: cover;
  border-radius: 6px;
  flex-shrink: 0;
}

.result-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: var(--pink-light);
}

.result-title {
  font-size: 20px;
  margin: 0;
}

.result-meta {
  color: var(--pink-mid);
  font-size: 14px;
  margin: 0;
}

.result-desc {
  font-size: 14px;
  line-height: 1.5;
  margin: 0;
  flex: 1;
}

.result-ratings {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

.rating {
  background: var(--accent);
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 13px;
}

@media (max-width: 600px) {
  .result-card {
    flex-direction: column;
  }
  .result-poster {
    width: 100%;
    height: 240px;
  }
}
</style>
