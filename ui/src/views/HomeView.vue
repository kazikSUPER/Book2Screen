<template>
  <div class="home">
    <section class="hero">
      <img :src="heroImg" :alt="STR.home.heroAlt" class="hero-image" />
      <button type="button" class="hero-btn" @click="goToTop">
        {{ STR.home.heroBtn }} <span class="hero-btn__accent">{{ STR.home.heroBtnAccent }}</span>
      </button>
    </section>

    <section class="popular">
      <h2 class="section-title">{{ STR.home.popularTitle }}</h2>

      <p v-if="isLoading" class="no-results">{{ STR.common.loading }}</p>

      <div v-else-if="errorMessage" class="no-results">
        <p>⚠ {{ errorMessage }}</p>
        <button type="button" class="retry-btn" @click="loadItems">{{ STR.common.retry }}</button>
      </div>

      <p v-else-if="filteredItems.length === 0" class="no-results">{{ STR.common.notFound }}</p>

      <div v-else class="cards-wrapper">
        <div ref="cardsRef" class="cards-scroll">
          <div v-for="item in filteredItems" :key="item.id" class="card" @click="goToItem(item)">
            <div class="card-poster">
              <img :src="item.poster" :alt="item.title" loading="lazy" />
            </div>
            <div class="card-info">
              <h3 class="card-title">{{ item.title }}</h3>
              <p class="card-meta">{{ STR.detail.bookYear }} {{ item.year }}</p>
              <p class="card-meta">{{ STR.detail.bookGenre }} {{ item.genre }}</p>
              <p class="card-meta">{{ STR.detail.bookCountry }} {{ item.country }}</p>
              <button type="button" class="card-btn">{{ STR.profile.view }}</button>
            </div>
          </div>
        </div>
        <button type="button" class="scroll-btn" aria-label="Далі" @click="scrollRight">›</button>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useFilter } from '../composables/useFilter';
import type { BookScreenItem } from '../services/types';
import { fetchWorks } from '../services/works';
import { extractErrorMessage } from '../services/error';
import heroImg from '../assets/Hero.png';
import { STR } from '../constants';

export default defineComponent({
  name: 'HomeView',
  setup() {
    const router = useRouter();
    const cardsRef = ref<HTMLElement | null>(null);

    const items = ref<BookScreenItem[]>([]);
    const isLoading = ref(false);
    const errorMessage = ref('');

    const { filteredItems } = useFilter(items);
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

    // SCRUM-67 — Hero-кнопка "Переглянути ТОП" веде на сторінку Top Lists.
    const goToTop = (): void => {
      router.push({ name: 'top' });
    };

    return {
      filteredItems,
      cardsRef,
      isLoading,
      errorMessage,
      loadItems,
      scrollRight,
      goToItem,
      goToTop,
      heroImg,
      STR,
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
  font-family: var(--font-body);
}

/* ── Hero ── */
.hero {
  width: 100%;
  border-radius: var(--radius-lg);
  overflow: hidden;
  position: relative;
}

.hero-image {
  width: 100%;
  height: auto;
  display: block;
  border-radius: var(--radius-lg);
}

.hero-btn {
  position: absolute;
  bottom: 5%;
  left: 50%;
  transform: translateX(-50%);
  background-color: var(--color-header);
  color: var(--text-on-dark);
  border: 2px solid var(--border-default);
  border-radius: var(--radius-md);
  padding: 20px 54px;
  font-size: 26px;
  font-family: var(--font-display);
  cursor: pointer;
  box-shadow: var(--shadow-md);
  transition: background 0.2s;
  white-space: nowrap;
}

.hero-btn:hover {
  background-color: var(--color-primary);
}

.hero-btn__accent {
  color: var(--color-primary);
  font-weight: 700;
}

.hero-btn:hover .hero-btn__accent {
  color: var(--text-on-dark);
}

/* ── Popular ── */
.section-title {
  font-size: 32px;
  font-weight: 400;
  font-family: var(--font-display);
  color: var(--text-on-light);
  margin: 0 0 16px;
}

.no-results {
  color: var(--text-muted);
  font-size: 15px;
  text-align: center;
  padding: 40px 0;
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
  scrollbar-color: var(--color-primary) transparent;
}

.cards-scroll::-webkit-scrollbar {
  height: 4px;
}
.cards-scroll::-webkit-scrollbar-thumb {
  background: var(--color-primary);
  border-radius: 2px;
}

/* ── Card ── */
.card {
  background-color: var(--color-card);
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  width: 300px;
  height: 500px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  cursor: pointer;
  transition: transform 0.2s;
  padding: 16px;
  box-sizing: border-box;
}

.card:hover {
  transform: translateY(-2px);
}

.card-poster {
  width: 200px;
  height: 260px;
  overflow: hidden;
  background: var(--color-header);
  border: 1px solid var(--border-card);
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
  font-family: var(--font-body);
  color: var(--text-on-dark);
  margin: 0;
  line-height: 1.3;
  text-align: center;
}

.card-meta {
  font-size: 14px;
  color: var(--text-on-dark);
  margin: 0 0 4px 0;
  align-self: flex-start;
  font-family: var(--font-body);
}

/* card-ratings — мертвий код видалено */

.card-btn {
  width: 200px;
  height: 50px;
  background-color: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-md);
  padding: 0;
  font-size: 16px;
  font-family: var(--font-display);
  font-weight: 400;
  cursor: pointer;
  box-shadow: var(--shadow-md);
  transition: background 0.2s;
  flex-shrink: 0;
}

.card-btn:hover {
  background-color: var(--color-primary-hover);
}

/* ── Arrow ── */
.scroll-btn {
  background: var(--color-input-bg);
  color: var(--text-on-light);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-pill);
  width: 44px;
  height: 40px;
  font-size: 22px;
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
  background: var(--color-panel-box);
  transform: scale(1.1);
}

/* ── Адаптив до мобільного (за STYLE_GUIDE: w-44 h-72 ≈ 176×288) ── */
@media (max-width: 768px) {
  .home {
    padding: 12px;
    gap: 16px;
  }

  .hero-btn {
    padding: 12px 28px;
    font-size: 18px;
  }

  .section-title {
    font-size: 22px;
    margin: 0 0 10px;
  }

  .cards-scroll {
    gap: 16px;
    padding: 4px;
  }

  .card {
    width: 176px;
    height: 288px;
    padding: 10px;
  }

  .card-poster {
    width: 100px;
    height: 130px;
  }

  .card-title {
    font-size: 14px;
  }

  .card-meta {
    font-size: 11px;
  }

  .card-btn {
    width: 150px;
    height: 38px;
    font-size: 13px;
  }

  .scroll-btn {
    width: 44px;
    height: 44px;
    font-size: 22px;
  }
}

@media (max-width: 380px) {
  .hero-btn {
    padding: 10px 22px;
    font-size: 15px;
  }

  .card {
    width: 160px;
    height: 264px;
  }

  .card-poster {
    width: 90px;
    height: 120px;
  }

  /* На зовсім малих ховаємо стрілку — горизонтальний скрол свайпом */
  .scroll-btn {
    display: none;
  }
}
</style>
