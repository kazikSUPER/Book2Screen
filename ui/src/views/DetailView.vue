<script lang="ts">
import { defineComponent, computed, ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { BookScreenItem } from '../services/types';
import { fetchWorkById } from '../services/works';
import { extractErrorMessage } from '../services/error';

export default defineComponent({
  name: 'DetailView',
  setup() {
    const route = useRoute();
    const router = useRouter();

    const item = ref<BookScreenItem | null>(null);
    const isLoading = ref(false);
    const errorMessage = ref('');

    const loadItem = async (): Promise<void> => {
      isLoading.value = true;
      errorMessage.value = '';
      try {
        item.value = await fetchWorkById(Number(route.params.id));
      } catch (err) {
        errorMessage.value = extractErrorMessage(err);
        item.value = null;
      } finally {
        isLoading.value = false;
      }
    };

    onMounted(loadItem);
    watch(() => route.params.id, loadItem);

    const winner = computed(() => {
      if (!item.value) return null;
      if (item.value.bookRating > item.value.filmRating) return 'book';
      if (item.value.filmRating > item.value.bookRating) return 'film';
      return 'draw';
    });

    const goBack = () => router.push({ name: 'home' });

    return { item, winner, isLoading, errorMessage, loadItem, goBack };
  },
});
</script>

<template>
  <div class="detail">
    <button class="back-btn" @click="goBack">← Назад</button>

    <p v-if="isLoading" class="not-found">Завантаження...</p>

    <div v-else-if="errorMessage" class="not-found">
      <p>⚠ {{ errorMessage }}</p>
      <button class="retry-btn" @click="loadItem">Повторити</button>
    </div>

    <div v-else-if="!item" class="not-found">
      <p>Елемент не знайдено 😔</p>
    </div>

    <div v-else class="detail-content">
      <h1 class="detail-title">{{ item.title }}</h1>
      <p class="detail-meta">{{ item.year }} · {{ item.genre }} · {{ item.country }}</p>
      <p class="detail-description">{{ item.description }}</p>

      <div class="compare-grid">
        <!-- Книга -->
        <div class="compare-card" :class="{ winner: winner === 'book' }">
          <div class="compare-header">
            <span class="compare-icon">📖</span>
            <h2 class="compare-label">Книга</h2>
            <span v-if="winner === 'book'" class="winner-badge">Переможець</span>
          </div>
          <img :src="item.poster" :alt="item.title" class="compare-poster" />
          <div class="compare-rating">
            <span class="rating-value">{{ item.bookRating }}</span>
            <span class="rating-max">/10</span>
          </div>
          <div class="rating-bar">
            <div class="rating-fill book-fill" :style="{ width: item.bookRating * 10 + '%' }"></div>
          </div>
        </div>

        <!-- VS -->
        <div class="vs-block">
          <span class="vs-text">VS</span>
          <p v-if="winner === 'draw'" class="draw-text">Нічия!</p>
        </div>

        <!-- Фільм -->
        <div class="compare-card" :class="{ winner: winner === 'film' }">
          <div class="compare-header">
            <span class="compare-icon">🎬</span>
            <h2 class="compare-label">Фільм</h2>
            <span v-if="winner === 'film'" class="winner-badge">Переможець</span>
          </div>
          <img :src="item.poster" :alt="item.title" class="compare-poster" />
          <div class="compare-rating">
            <span class="rating-value">{{ item.filmRating }}</span>
            <span class="rating-max">/10</span>
          </div>
          <div class="rating-bar">
            <div class="rating-fill film-fill" :style="{ width: item.filmRating * 10 + '%' }"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.detail {
  padding: 24px;
  font-family: 'Georgia', serif;
  color: var(--pink-light);
  max-width: 900px;
  margin: 0 auto;
}

.back-btn {
  background: none;
  border: 1px solid var(--accent);
  color: var(--pink-light);
  font-family: 'Georgia', serif;
  font-size: 14px;
  padding: 6px 16px;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 24px;
  transition: background 0.2s;
}

.back-btn:hover {
  background-color: var(--accent);
}

.not-found {
  text-align: center;
  padding: 60px 0;
  color: var(--pink-mid);
  font-size: 16px;
}

.retry-btn {
  margin-top: 12px;
  padding: 8px 20px;
  background: var(--accent);
  color: var(--pink-light);
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-family: 'Georgia', serif;
}

.retry-btn:hover {
  background: #a82040;
}

.detail-title {
  font-size: 26px;
  font-weight: 700;
  color: var(--pink-light);
  margin: 0 0 8px;
}

.detail-meta {
  font-size: 14px;
  color: var(--pink-mid);
  margin: 0 0 12px;
}

.detail-description {
  font-size: 15px;
  color: var(--pink-light);
  line-height: 1.7;
  margin: 0 0 32px;
  max-width: 620px;
}

.compare-grid {
  display: flex;
  align-items: center;
  gap: 16px;
}

.compare-card {
  flex: 1;
  background-color: var(--dark-card);
  border: 2px solid var(--accent);
  border-radius: 12px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  transition:
    border-color 0.2s,
    box-shadow 0.2s;
}

.compare-card.winner {
  border-color: #f0c040;
  box-shadow: 0 0 20px rgba(240, 192, 64, 0.3);
}

.compare-header {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  justify-content: center;
  flex-wrap: wrap;
}

.compare-icon {
  font-size: 24px;
}

.compare-label {
  font-size: 18px;
  font-weight: 700;
  color: var(--pink-light);
  margin: 0;
}

.winner-badge {
  background-color: #f0c040;
  color: #311620;
  font-size: 11px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 10px;
}

.compare-poster {
  width: 140px;
  height: 200px;
  object-fit: cover;
  border-radius: 8px;
  border: 1px solid var(--accent);
}

.compare-rating {
  display: flex;
  align-items: baseline;
  gap: 2px;
}

.rating-value {
  font-size: 36px;
  font-weight: 700;
  color: var(--pink-light);
}

.rating-max {
  font-size: 16px;
  color: var(--pink-mid);
}

.rating-bar {
  width: 100%;
  height: 6px;
  background-color: #311620;
  border-radius: 3px;
  overflow: hidden;
}

.rating-fill {
  height: 100%;
  border-radius: 3px;
  transition: width 0.4s ease;
}

.book-fill {
  background-color: var(--pink-mid);
}

.film-fill {
  background-color: var(--accent);
}

.vs-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.vs-text {
  font-size: 28px;
  font-weight: 700;
  color: var(--accent);
}

.draw-text {
  font-size: 12px;
  color: var(--pink-mid);
  margin: 0;
  text-align: center;
}
</style>
