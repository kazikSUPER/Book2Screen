<script lang="ts">
import { defineComponent, ref, computed, watch, type Ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useFilter } from '../hooks/useFilter';
import { ALL_ITEMS } from '../services/items';

export default defineComponent({
  name: 'SearchView',
  setup() {
    const route = useRoute();
    const router = useRouter();

    const searchQuery = ref((route.query.q as string) || '');
    const selectedGenre: Ref<string | null> = ref(null);
    const selectedCountry: Ref<string | null> = ref(null);

    const { filteredItems } = useFilter(ALL_ITEMS, searchQuery, selectedGenre, selectedCountry);

    // Якщо URL змінився (новий пошук) — оновити query
    watch(
      () => route.query.q,
      (newQuery) => {
        searchQuery.value = (newQuery as string) || '';
      }
    );

    const resultsCount = computed(() => filteredItems.value.length);

    const goToItem = (id: number) => {
      router.push({ name: 'detail', params: { id } });
    };

    return { searchQuery, filteredItems, resultsCount, goToItem };
  },
});
</script>

<template>
  <div class="search-page">
    <div class="search-header">
      <h1 class="search-title">Результати пошуку</h1>
      <p class="search-info">
        За запитом <strong>"{{ searchQuery }}"</strong> знайдено:
        <strong>{{ resultsCount }}</strong>
      </p>
    </div>

    <div v-if="resultsCount === 0" class="no-results">
      <p>Нічого не знайдено 😔</p>
      <p class="hint">Спробуй інший запит</p>
    </div>

    <div v-else class="results-grid">
      <div v-for="item in filteredItems" :key="item.id" class="result-card" @click="goToItem(item.id)">
        <div class="result-poster">
          <img :src="item.poster" :alt="item.title" />
        </div>
        <div class="result-info">
          <h3 class="result-title">{{ item.title }}</h3>
          <p class="result-meta">Рік: {{ item.year }}</p>
          <p class="result-meta">Жанр: {{ item.genre }}</p>
          <p class="result-meta">Країна: {{ item.country }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.search-page {
  padding: 24px;
  font-family: 'Inter', sans-serif;
  color: white;
}

.search-header {
  margin-bottom: 24px;
}

.search-title {
  font-size: 28px;
  font-weight: 400;
  font-family: 'JetBrains Mono', monospace;
  color: #000;
  margin: 0 0 8px;
}

.search-info {
  font-size: 16px;
  color: #311620;
  margin: 0;
}

.no-results {
  text-align: center;
  padding: 60px 20px;
  color: #311620;
}

.no-results p {
  margin: 4px 0;
  font-size: 18px;
}

.hint {
  font-size: 14px !important;
  color: #8e182f;
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 20px;
}

.result-card {
  background-color: #3d0000;
  border: 1px white solid;
  border-radius: 10px;
  box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.3);
  padding: 12px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  transition: transform 0.2s;
}

.result-card:hover {
  transform: translateY(-3px);
}

.result-poster {
  width: 160px;
  height: 220px;
  overflow: hidden;
  background: #311620;
  border: 1px white solid;
}

.result-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.result-info {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.result-title {
  font-size: 14px;
  color: white;
  margin: 0 0 4px;
  text-align: center;
  line-height: 1.3;
}

.result-meta {
  font-size: 12px;
  color: #e0e0e0;
  margin: 0;
}
</style>
