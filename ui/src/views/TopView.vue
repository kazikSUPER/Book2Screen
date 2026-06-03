<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useFilter } from '../composables/useFilter';
import type { BookScreenItem } from '../services/types';
import { fetchWorks } from '../services/works';
import { extractErrorMessage } from '../services/error';
import TopFilterBar from '../components/TopFilterBar.vue';
import WorkCard from '../components/WorkCard.vue';
import { STR } from '../constants';

const t = STR.top;

/**
 * SCRUM-67 (US 3.1) — Top Lists.
 *
 * За Figma (Frame 7) сторінка має ДВІ секції:
 *   • "Найкращі адаптації" — твори, де bookRating/filmRating близькі (вдала екранізація)
 *   • "Найгірші адаптації" — твори з великою розбіжністю рейтингів (невдала екранізація)
 *
 * Над секціями — горизонтальна панель фільтрів і чекбокс
 * "Лише з картою відмінностей". Фільтрація — на стороні клієнта (useFilter).
 */

const items = ref<BookScreenItem[]>([]);
const isLoading = ref(false);
const errorMessage = ref('');

const { filteredItems } = useFilter(items);

// "Найкращі" — за середнім рейтингом (книги + фільму), спадання.
const bestAdaptations = computed<BookScreenItem[]>(() => {
  return [...filteredItems.value]
    .sort((a, b) => (b.bookRating + b.filmRating) / 2 - (a.bookRating + a.filmRating) / 2)
    .slice(0, 8);
});

// "Найгірші" — за розривом між рейтингом книги і екранізації (більший розрив = гірша адаптація).
const worstAdaptations = computed<BookScreenItem[]>(() => {
  return [...filteredItems.value]
    .sort((a, b) => Math.abs(b.bookRating - b.filmRating) - Math.abs(a.bookRating - a.filmRating))
    .slice(0, 8);
});

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

    <p v-if="isLoading" class="top-page__status">{{ STR.common.loading }}</p>

    <div v-else-if="errorMessage" class="top-page__status">
      <p>{{ errorMessage }}</p>
      <button type="button" class="top-page__retry" @click="loadItems">
        {{ STR.common.retry }}
      </button>
    </div>

    <p v-else-if="filteredItems.length === 0" class="top-page__status">
      {{ t.emptyByFilters }}
    </p>

    <template v-else>
      <section class="top-section">
        <header class="top-section__head">
          <h2 class="top-section__title">{{ t.bestTitle }}</h2>
          <button type="button" class="top-section__more" :aria-label="STR.common.showMore">&rsaquo;</button>
        </header>
        <div class="top-section__row">
          <WorkCard v-for="item in bestAdaptations" :key="`best-${item.id}`" :item="item" />
        </div>
      </section>

      <section class="top-section">
        <header class="top-section__head">
          <h2 class="top-section__title">{{ t.worstTitle }}</h2>
          <button type="button" class="top-section__more" :aria-label="STR.common.showMore">&rsaquo;</button>
        </header>
        <div class="top-section__row">
          <WorkCard v-for="item in worstAdaptations" :key="`worst-${item.id}`" :item="item" />
        </div>
      </section>
    </template>
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

.top-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.top-section__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.top-section__title {
  font-size: 28px;
  font-family: var(--font-display);
  font-weight: 400;
  color: var(--text-on-light);
  margin: 0;
}

.top-section__more {
  background: var(--color-input-bg);
  color: var(--text-on-light);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-pill);
  width: 36px;
  height: 36px;
  font-size: 22px;
  line-height: 1;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: background 0.15s;
}

.top-section__more:hover {
  background: var(--color-panel-box);
}

.top-section__row {
  display: flex;
  gap: 20px;
  overflow-x: auto;
  scroll-behavior: smooth;
  padding-bottom: 6px;
  scrollbar-width: thin;
  scrollbar-color: var(--color-primary) transparent;
}

.top-section__row::-webkit-scrollbar {
  height: 6px;
}
.top-section__row::-webkit-scrollbar-thumb {
  background: var(--color-primary);
  border-radius: 3px;
}

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
  .top-section__title {
    font-size: 22px;
  }
}
</style>
