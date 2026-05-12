<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useFilter } from '../hooks/useFilter';
import type { BookScreenItem } from '../services/types';
import { fetchWorks } from '../services/works';
import { extractErrorMessage } from '../services/error';
import TopFilterBar from '../components/TopFilterBar.vue';
import WorkCard from '../components/WorkCard.vue';

/**
 * SCRUM-67 (US 3.1) — Top Lists.
 * Сторінка "Найкращі адаптації": горизонтальна панель фільтрів +
 * сітка карток у стилі Figma.
 *
 * Фільтрація — на стороні клієнта через useFilter (зчитує з useFiltersStore).
 * Дані — з fetchWorks(); fallback на mock у services/works.ts.
 */

const items = ref<BookScreenItem[]>([]);
const isLoading = ref(false);
const errorMessage = ref('');

const { filteredItems } = useFilter(items);

async function loadItems(): Promise<void> {
  isLoading.value = true;
  errorMessage.value = '';
  try {
    items.value = await fetchWorks();
  } catch (err) {
    errorMessage.value = extractErrorMessage(err);
    items.value = [];
  } finally {
    isLoading.value = false;
  }
}

onMounted(loadItems);
</script>

<template>
  <div class="top-page">
    <TopFilterBar />

    <h1 class="top-page__title">Найкращі адаптації</h1>

    <p v-if="isLoading" class="top-page__status">Завантаження…</p>

    <div v-else-if="errorMessage" class="top-page__status">
      <p>⚠ {{ errorMessage }}</p>
      <button class="top-page__retry" @click="loadItems">Повторити</button>
    </div>

    <p v-else-if="filteredItems.length === 0" class="top-page__status">
      За обраними фільтрами нічого не знайдено
    </p>

    <div v-else class="top-page__grid">
      <WorkCard v-for="item in filteredItems" :key="item.id" :item="item" />
    </div>
  </div>
</template>

<style scoped>
.top-page {
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  font-family: var(--font-body);
}

.top-page__title {
  font-size: 32px;
  font-family: var(--font-display);
  font-weight: 400;
  color: var(--text-on-light);
  margin: 0;
}

/* ── Grid карток ─────────────────────────────────────────── */
.top-page__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 24px;
}

@media (min-width: 1600px) {
  .top-page__grid {
    grid-template-columns: repeat(5, 1fr);
  }
}

/* ── Status ─────────────────────────────────────────────── */
.top-page__status {
  text-align: center;
  padding: 40px 0;
  color: var(--text-muted);
  font-size: 16px;
  font-family: var(--font-display);
}

.top-page__retry {
  margin-top: 12px;
  padding: 8px 20px;
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: none;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 14px;
  font-family: var(--font-display);
  transition: background 0.2s;
}

.top-page__retry:hover {
  background: var(--color-primary-hover);
}

@media (max-width: 768px) {
  .top-page {
    padding: 12px;
    gap: 16px;
  }
  .top-page__title {
    font-size: 24px;
  }
  .top-page__grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 16px;
  }
}
</style>
