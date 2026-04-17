<template>
  <div class="home">
    <section class="hero">
      <span class="hero-emoji">📖</span>
      <div class="hero-center">
        <h1 class="hero-title">Книга чи фільм – що краще?</h1>
        <p class="hero-subtitle">
          Платформа, де ти можеш оцінити екранізації,<br />
          залишити відгук і побачити думку інших
        </p>
        <button class="hero-btn">Переглянути <strong>ТОП</strong></button>
      </div>
      <span class="hero-emoji">🎬</span>
    </section>

    <section class="popular">
      <h2 class="section-title">Популярні порівняння</h2>
      <div class="cards-wrapper">
        <div class="cards-scroll" ref="cardsRef">
          <div v-for="(item, index) in popularItems" :key="index" class="card">
            <div class="card-poster">
              <img :src="item.poster" :alt="item.title" />
            </div>
            <div class="card-info">
              <h3 class="card-title">{{ item.title }}</h3>
              <p class="card-meta">Рік: {{ item.year }}</p>
              <p class="card-meta">Жанр: {{ item.genre }}</p>
              <p class="card-meta">Країна: {{ item.country }}</p>
              <button class="card-btn" @click="goToItem(item)">Переглянути</button>
            </div>
          </div>
        </div>
        <button class="scroll-btn" @click="scrollRight" aria-label="Далі">›</button>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
import { useRouter } from 'vue-router'

interface PopularItem {
  title: string
  year: number
  genre: string
  country: string
  poster: string
  id?: number
}

export default defineComponent({
  name: 'HomeView',
  setup() {
    const router = useRouter()
    const cardsRef = ref<HTMLElement | null>(null)

    const popularItems: PopularItem[] = [
      { title: 'Гаррі Поттер і філософський камінь', year: 2001, genre: 'Фентезі, пригоди', country: 'Велика Британія', poster: 'https://m.media-amazon.com/images/M/MV5BNmQ0ODBhMjUtNDRhOC00MGQzLTk5MTAtZTJjOWQ1NjkxZmQ1XkEyXkFqcGdeQXVyNDUyOTg3Njg@._V1_SX300.jpg' },
      { title: 'Гаррі Поттер і філософський камінь', year: 2001, genre: 'Фентезі, пригоди', country: 'Велика Британія', poster: 'https://m.media-amazon.com/images/M/MV5BNmQ0ODBhMjUtNDRhOC00MGQzLTk5MTAtZTJjOWQ1NjkxZmQ1XkEyXkFqcGdeQXVyNDUyOTg3Njg@._V1_SX300.jpg' },
      { title: 'Гаррі Поттер і філософський камінь', year: 2001, genre: 'Фентезі, пригоди', country: 'Велика Британія', poster: 'https://m.media-amazon.com/images/M/MV5BNmQ0ODBhMjUtNDRhOC00MGQzLTk5MTAtZTJjOWQ1NjkxZmQ1XkEyXkFqcGdeQXVyNDUyOTg3Njg@._V1_SX300.jpg' },
      { title: 'Гаррі Поттер і філософський камінь', year: 2001, genre: 'Фентезі, пригоди', country: 'Велика Британія', poster: 'https://m.media-amazon.com/images/M/MV5BNmQ0ODBhMjUtNDRhOC00MGQzLTk5MTAtZTJjOWQ1NjkxZmQ1XkEyXkFqcGdeQXVyNDUyOTg3Njg@._V1_SX300.jpg' },
    ]

    const scrollRight = (): void => {
      if (cardsRef.value) cardsRef.value.scrollBy({ left: 320, behavior: 'smooth' })
    }

    const goToItem = (item: PopularItem): void => {
      router.push({ name: 'detail', params: { id: item.id ?? 1 } })
    }

    return { popularItems, cardsRef, scrollRight, goToItem }
  },
})
</script>

<style scoped>
.home {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  font-family: 'Georgia', serif;
}

/* ── Hero ── */
.hero {
  background: linear-gradient(135deg, #F7CCCC 0%, #E4AFAF 60%, #c9909a 100%);
  border-radius: 12px;
  padding: 36px 28px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.hero-emoji {
  font-size: 64px;
  line-height: 1;
  user-select: none;
}

.hero-center { text-align: center; flex: 1; }

.hero-title {
  font-size: 22px;
  font-weight: 700;
  color: #311620;
  margin: 0 0 10px;
  line-height: 1.3;
}

.hero-subtitle {
  font-size: 14px;
  color: #311620;
  margin: 0 0 20px;
  line-height: 1.7;
}

.hero-btn {
  background-color: #311620;
  color: #F7CCCC;
  border: none;
  border-radius: 6px;
  padding: 10px 28px;
  font-size: 14px;
  font-family: 'Georgia', serif;
  cursor: pointer;
  transition: background 0.2s, transform 0.15s;
}

.hero-btn:hover {
  background-color: #3D0000;
  transform: translateY(-1px);
}

/* ── Popular ── */
.section-title {
  font-size: 20px;
  font-weight: 600;
  color: #F7CCCC;
  margin: 0 0 16px;
}

.cards-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.cards-scroll {
  display: flex;
  gap: 16px;
  overflow-x: auto;
  scroll-behavior: smooth;
  padding-bottom: 8px;
  flex: 1;
  scrollbar-width: thin;
  scrollbar-color: #8E182F transparent;
}

.cards-scroll::-webkit-scrollbar { height: 4px; }
.cards-scroll::-webkit-scrollbar-thumb { background: #8E182F; border-radius: 2px; }

/* ── Card ── */
.card {
  background-color: #3D0000;
  border: 1px solid #8E182F;
  border-radius: 10px;
  overflow: hidden;
  min-width: 200px;
  max-width: 200px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  transition: transform 0.2s, box-shadow 0.2s;
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(142, 24, 47, 0.45);
}

.card-poster {
  width: 100%;
  height: 230px;
  overflow: hidden;
  background: #311620;
}

.card-poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.card-info {
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
}

.card-title {
  font-size: 14px;
  font-weight: 700;
  color: #F7CCCC;
  margin: 0 0 6px;
  line-height: 1.4;
  text-align: center;
}

.card-meta {
  font-size: 12px;
  color: #E4AFAF;
  margin: 0;
}

.card-btn {
  margin-top: 12px;
  background-color: #8E182F;
  color: #F7CCCC;
  border: none;
  border-radius: 6px;
  padding: 8px 12px;
  font-size: 13px;
  font-family: 'Georgia', serif;
  cursor: pointer;
  width: 100%;
  transition: background 0.2s;
}

.card-btn:hover { background-color: #a82040; }

/* ── Arrow ── */
.scroll-btn {
  background: #8E182F;
  color: #F7CCCC;
  border: none;
  border-radius: 50%;
  width: 38px;
  height: 38px;
  font-size: 22px;
  cursor: pointer;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s, transform 0.15s;
}

.scroll-btn:hover {
  background: #a82040;
  transform: scale(1.1);
}
</style>
