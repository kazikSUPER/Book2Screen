<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useFilter } from '../composables/useFilter';
import type { BookScreenItem } from '../services/types';
import { fetchWorks } from '../services/works';
import { extractErrorMessage } from '../services/error';
import { useFiltersStore } from '../state/filters';
import { STR } from '../constants';
import WorkCard from '../components/WorkCard.vue';

/**
 * SCRUM-66 (US 2.1) — Search Results.
 *
 * Пошук виконується по бекенду (search=…), додаткова клієнтська фільтрація
 * (жанр, країна, рік, рейтинг) — через useFilter.
 *
 * URL ?q=… синхронізується з filtersStore.searchQuery, щоб шерінг лінків
 * з результатами працював коректно.
 */

const route = useRoute();
const filters = useFiltersStore();
const t = STR.search;

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
const searchQuery = computed(() => filters.searchQuery);
</script>

<template>
  <div class="search-page">
    <h1 class="page-title">{{ t.pageTitle }}</h1>

    <p v-if="searchQuery" class="search-info">
      {{ t.queryLabel }} <strong>"{{ searchQuery }}"</strong>
      <span v-if="!isLoading">{{ t.foundLabel(resultsCount) }}</span>
    </p>

    <p v-if="isLoading" class="status">{{ STR.common.loading }}</p>

    <div v-else-if="errorMessage" class="status">
      <p>⚠ {{ errorMessage }}</p>
      <button type="button" class="retry-btn" @click="loadItems">{{ STR.common.retry }}</button>
    </div>

    <p v-else-if="filteredItems.length === 0" class="status">{{ STR.common.notFound }}</p>

    <div v-else class="results-grid">
      <WorkCard v-for="item in filteredItems" :key="item.id" :item="item" />
    </div>
  </div>
</template>

<style scoped>
.search-page {
  padding: 24px;
  font-family: var(--font-body);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.page-title {
  font-size: 28px;
  font-family: var(--font-display);
  font-weight: 400;
  color: var(--text-on-light);
  margin: 0;
}

.search-info {
  color: var(--text-on-light);
  font-size: 14px;
  margin: 0 0 16px;
  font-family: var(--font-body);
}

.status {
  text-align: center;
  padding: 60px 0;
  color: var(--text-muted);
  font-size: 16px;
  font-family: var(--font-display);
}

.retry-btn {
  margin-top: 12px;
  padding: 8px 20px;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 14px;
  font-family: var(--font-display);
}

.retry-btn:hover {
  background: var(--color-primary-hover);
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 24px;
  margin-top: 8px;
}

@media (max-width: 768px) {
  .search-page {
    padding: 12px;
  }
  .results-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 16px;
  }
}
</style>
