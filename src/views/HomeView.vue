<template>
  <div class="home">
    <section class="hero">
      <img :src="heroImg" alt="Книга чи фільм – що краще?" class="hero-image" />
      <button class="hero-btn">Переглянути <strong>ТОП</strong></button>
    </section>

    <section class="popular">
      <h2 class="section-title">Популярні порівняння</h2>

      <p v-if="isLoading" class="no-results">Завантаження...</p>

      <div v-else-if="errorMessage" class="no-results">
        <p>⚠ {{ errorMessage }}</p>
        <button class="retry-btn" @click="loadItems">Повторити</button>
      </div>

      <p v-else-if="filteredItems.length === 0" class="no-results">Нічого не знайдено 😔</p>

      <div v-else class="cards-wrapper">
        <div class="cards-scroll" ref="cardsRef">
          <div v-for="item in filteredItems" :key="item.id" class="card" @click="goToItem(item)">
            <div class="card-poster">
              <img :src="item.poster" :alt="item.title" />
            </div>
            <div class="card-info">
              <h3 class="card-title">{{ item.title }}</h3>
              <p class="card-meta">Рік: {{ item.year }}</p>
              <p class="card-meta">Жанр: {{ item.genre }}</p>
              <p class="card-meta">Країна: {{ item.country }}</p>
              <div class="card-ratings">
                <span class="rating book">📖 {{ item.bookRating }}</span>
                <span class="rating film">🎬 {{ item.filmRating }}</span>
              </div>
              <button class="card-btn">Переглянути</button>
            </div>
          </div>
        </div>
        <button class="scroll-btn" @click="scrollRight" aria-label="Далі">›</button>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, inject, onMounted, type Ref } from 'vue';
import { useRouter } from 'vue-router';
import { useFilter } from '../hooks/useFilter';
import type { BookScreenItem } from '../services/types';
import { fetchWorks } from '../services/works';
import { extractErrorMessage } from '../services/error';
import heroImg from '../assets/Hero.png';

export default defineComponent({
  name: 'HomeView',
  setup() {
    const router = useRouter();
    const cardsRef = ref<HTMLElement | null>(null);

    const searchQuery = inject<Ref<string>>('searchQuery', ref(''));
    const selectedGenre = inject<Ref<string | null>>('selectedGenre', ref(null));
    const selectedCountry = inject<Ref<string | null>>('selectedCountry', ref(null));

    const items = ref<BookScreenItem[]>([]);
    const isLoading = ref(false);
    const errorMessage = ref('');

    const { filteredItems } = useFilter(items, searchQuery, selectedGenre, selectedCountry);

    const loadItems = async (): Promise<void> => {
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
    };

    onMounted(loadItems);

    const scrollRight = (): void => {
      if (cardsRef.value) cardsRef.value.scrollBy({ left: 380, behavior: 'smooth' });
    };

    const goToItem = (item: BookScreenItem): void => {
      router.push({ name: 'detail', params: { id: item.id } });
    };

    return {
      filteredItems,
      cardsRef,
      isLoading,
      errorMessage,
      loadItems,
      scrollRight,
      goToItem,
      heroImg,
    };
  },
});
</script>

<style scoped>
.home {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  font-family: 'Inter', sans-serif;
}

/* ── Hero ── */
.hero {
  width: 100%;
  border-radius: 12px;
  overflow: hidden;
  position: relative;
}

.hero-image {
  width: 100%;
  height: auto;
  display: block;
  border-radius: 12px;
}

.hero-btn {
  position: absolute;
  bottom: 5%;
  left: 50%;
  transform: translateX(-50%);
  background-color: #311620;
  color: #f7cccc;
  border: 2px black solid;
  border-radius: 10px;
  padding: 20px 54px;
  font-size: 26px;
  font-family: 'JetBrains Mono', monospace;
  cursor: pointer;
  box-shadow: 5px 4px 4px rgba(0, 0, 0, 0.25);
  transition: background 0.2s;
  white-space: nowrap;
}

.hero-btn strong {
  font-size: 22px;
}

.hero-btn strong {
  font-size: 27px;
}

/* ── Popular ── */
.section-title {
  font-size: 32px;
  font-weight: 400;
  font-family: 'JetBrains Mono', monospace;
  color: #000;
  margin: 0 0 16px;
}

.no-results {
  color: var(--pink-mid);
  font-size: 15px;
  text-align: center;
  padding: 40px 0;
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

.cards-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.cards-scroll {
  display: flex;
  gap: 35px;
  overflow-x: auto;
  scroll-behavior: smooth;
  padding: 5px;
  flex: 1;
  scrollbar-width: thin;
  scrollbar-color: #8e182f transparent;
}

.cards-scroll::-webkit-scrollbar {
  height: 4px;
}
.cards-scroll::-webkit-scrollbar-thumb {
  background: #8e182f;
  border-radius: 2px;
}

/* ── Card ── */
.card {
  background-color: #3d0000;
  border: 1px white solid;
  border-radius: 12px;
  box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.3);
  width: 300px; /* Фіксована ширина замість max-width */
  height: 500px; /* Збільшена висота */
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  cursor: pointer;
  transition: transform 0.2s;
  padding: 16px;
  box-sizing: border-box;
}

.card-poster {
  width: 200px;
  height: 260px;
  overflow: hidden;
  background: #311620;
  border: 1px white solid;
  flex-shrink: 0;
}

.card-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.card-info {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  flex: 1;
  justify-content: center;
  padding: 0 10px;
}

.card-title {
  font-size: 18px;
  font-weight: 400;
  font-family: 'Inter', sans-serif;
  color: white;
  margin: 0;
  line-height: 1.3;
  text-align: center;
}

.card-meta {
  font-size: 16px;
  color: #e0e0e0;
  margin: 0 0 4px 0;
  align-self: flex-start; /* Вирівнювання по лівому краю, як на макеті */
}
.card-ratings {
  display: none;
}

.card-btn {
  width: 200px;
  height: 50px;
  background-color: #8e182f;
  color: white;
  border: 0px #3d0000 solid;
  border-radius: 10px;
  padding: 0;
  font-size: 16px;
  font-family: 'JetBrains Mono', monospace;
  font-weight: 400;
  cursor: pointer;
  box-shadow: 5px 4px 4px rgba(0, 0, 0, 0.25);
  transition: background 0.2s;
  flex-shrink: 0;
}

.card-btn:hover {
  background-color: #a82040;
}

/* ── Arrow ── */
.scroll-btn {
  background: #f7cccc;
  color: black;
  border: none;
  border-radius: 50%;
  width: 62px;
  height: 63px;
  font-size: 26px;
  cursor: pointer;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition:
    background 0.2s,
    transform 0.15s;
}

.scroll-btn:hover {
  background: #e4afaf;
  transform: scale(1.1);
}
</style>
