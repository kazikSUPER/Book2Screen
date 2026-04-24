<script setup lang="ts">
import { ref, provide, onMounted } from 'vue';
import { RouterView, RouterLink, useRouter } from 'vue-router';
import LoginModal from './components/LoginModal.vue';
import RegisterModal from './components/RegisterModal.vue';
import ResetPasswordModal from './components/ResetPasswordModal.vue';
import logoImg from './assets/Hero.png';
import { checkHealth } from './services/health';
import { useUserStore } from './state/user';

const router = useRouter();
const userStore = useUserStore();

// ── Health-check при старті застосунку ──────────────
// Етап 3, Крок 3: фронт пінгує /health, щоб одразу бачити стан бекенду.
const backendStatus = ref<'checking' | 'up' | 'down'>('checking');

onMounted(async () => {
  try {
    await checkHealth();
    backendStatus.value = 'up';

    console.info('[Book2Screen] Backend /health: OK');
  } catch (err) {
    backendStatus.value = 'down';

    console.warn('[Book2Screen] Backend /health: FAILED', err);
  }
});

// ── Search ──────────────────────────────────────────
const searchQuery = ref('');
const onSearchSubmit = () => {
  if (searchQuery.value.trim()) {
    router.push({ name: 'search', query: { q: searchQuery.value.trim() } });
  }
};

// ── Modals ──────────────────────────────────────────
type ModalType = 'login' | 'register' | 'reset' | null;
const activeModal = ref<ModalType>(null);

// ── Filters ─────────────────────────────────────────
const genres: string[] = [
  'Комедія',
  'Драма',
  'Фантастика',
  'Фентезі',
  'Жахи',
  'Детектив',
  'Кримінал',
  'Пригоди',
  'Історичні',
  'Біографічні',
  'Документальні',
];

const countries: string[] = [
  'Україна',
  'США',
  'Велика Британія',
  'Канада',
  'Греція',
  'Італія',
  'Туреччина',
  'Іспанія',
  'Німеччина',
  'Японія',
  'Швеція',
];

const selectedGenre = ref<string | null>(null);
const selectedCountry = ref<string | null>(null);

provide('searchQuery', searchQuery);
provide('selectedGenre', selectedGenre);
provide('selectedCountry', selectedCountry);

// ── Auth button click ───────────────────────────────
const handleAuthClick = () => {
  if (userStore.isAuthenticated) {
    userStore.logout();
  } else {
    activeModal.value = 'login';
  }
};
</script>

<template>
  <div class="app-wrapper">
    <!-- Банер статусу бекенду (показується тільки якщо сервер не відповідає) -->
    <div v-if="backendStatus === 'down'" class="backend-status-banner">
      ⚠ Backend недоступний. Дані не завантажуються.
    </div>

    <header class="header">
      <RouterLink to="/" class="logo">
        <img :src="logoImg" alt="Book2Screen" class="logo-img" />
      </RouterLink>

      <div class="search-bar">
        <input v-model="searchQuery" type="text" placeholder="Пошук..." @keyup.enter="onSearchSubmit" />
        <span class="search-icon">🔍</span>
      </div>

      <button
        class="login-btn"
        :title="userStore.isAuthenticated ? `Вийти (${userStore.email})` : 'Вхід'"
        @click="handleAuthClick"
      >
        {{ userStore.isAuthenticated ? '✕' : '→' }}
      </button>
    </header>

    <div class="main-layout">
      <aside class="sidebar">
        <div class="filter-box">
          <h3 class="filter-title">Жанри</h3>
          <ul class="filter-list">
            <li
              v-for="genre in genres"
              :key="genre"
              class="filter-item"
              :class="{ active: selectedGenre === genre }"
              @click="selectedGenre = selectedGenre === genre ? null : genre"
            >
              {{ genre }}
            </li>
          </ul>
        </div>

        <div class="filter-box">
          <h3 class="filter-title">Країна</h3>
          <ul class="filter-list">
            <li
              v-for="country in countries"
              :key="country"
              class="filter-item"
              :class="{ active: selectedCountry === country }"
              @click="selectedCountry = selectedCountry === country ? null : country"
            >
              {{ country }}
            </li>
          </ul>
        </div>
      </aside>

      <main class="content">
        <RouterView />
      </main>
    </div>

    <LoginModal
      v-if="activeModal === 'login'"
      @close="activeModal = null"
      @open-register="activeModal = 'register'"
      @open-reset="activeModal = 'reset'"
    />
    <RegisterModal v-if="activeModal === 'register'" @close="activeModal = null" @success="activeModal = null" />
    <ResetPasswordModal v-if="activeModal === 'reset'" @close="activeModal = null" />
  </div>
</template>

<style>
:root {
  --pink-light: #f7cccc;
  --pink-mid: #e4afaf;
  --dark-bg: #ffffff;
  --dark-card: #3d0000;
  --accent: #8e182f;
}

*,
*::before,
*::after {
  box-sizing: border-box;
}

body {
  margin: 0;
  padding: 0;
  font-family: 'Georgia', serif;
  background-color: var(--dark-bg);
  color: var(--pink-light);
}

.app-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

/* ── Backend status banner ── */
.backend-status-banner {
  background: #cc0000;
  color: white;
  text-align: center;
  padding: 8px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 14px;
}

/* ── Header ── */
.header {
  background-color: #311620;
  height: 80px; /* Трохи вище для пропорційності */
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  position: relative; /* Важливо для абсолютного центрування пошуку */
  flex-shrink: 0;
}

.logo {
  background-color: #f7cccc;
  color: black;
  text-decoration: none;
  padding: 4px 8px;
  border-radius: 8px;
  font-family: 'Ink Free', cursive;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 72px; /* Зменшено, щоб не вилазило за межі шапки */
  width: 195px;
}

.logo-img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.search-bar {
  width: 700px;
  flex-shrink: 0;
  position: relative;
  display: flex;
  align-items: center;
}

.search-bar input {
  width: 100%;
  padding: 9px 40px 9px 16px;
  border-radius: 15px;
  border: 1px black solid;
  background-color: #f7cccc;
  color: var(--dark-card);
  font-size: 14px;
  font-family: 'Georgia', serif;
  outline: none;
}

.search-bar input::placeholder {
  color: var(--accent);
}

.search-icon {
  position: absolute;
  right: 12px;
  font-size: 16px;
  pointer-events: none;
}

.login-btn {
  color: var(--pink-light);
  background: none;
  border: none;
  cursor: pointer;
  text-decoration: none;
  font-size: 26px;
  flex-shrink: 0;
  transition: color 0.2s;
}
.login-btn:hover {
  color: #fff;
}

/* ── Layout ── */
.main-layout {
  display: flex;
  flex: 1;
  padding: 10px; /* Створює ту саму білу рамку навколо всього, як у Фігмі */
  margin: 0 auto; /* Центрує контент на дуже широких екранах */
  width: 100%;
}

/* ── Sidebar ── */
.sidebar {
  width: 250px;
  flex-shrink: 0;
  background-color: #f7cccc;
  border: 1px black solid;
  border-radius: 5px;
  padding: 20px 20px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  overflow-y: auto;
}

.filter-box {
  background-color: #e4afaf;
  border: 1px black solid;
  border-radius: 10px;
  padding: 5px 15px;
  box-shadow: 3px 4px 4px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: flex-start;
  gap: 4px;
}

.filter-title {
  font-size: 25px;
  font-family: 'JetBrains Mono', monospace;
  color: #311620;
  margin: 0 0 4px;
  align-self: stretch;
  padding-left: 2px;
}

.filter-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  width: 100%;
}

.filter-item {
  font-size: 20px;
  font-family: 'JetBrains Mono', monospace;
  color: #311620;
  cursor: pointer;
  padding: 2px 4px;
  border-radius: 4px;
  transition:
    background 0.15s,
    color 0.15s;
}

.filter-item:hover {
  background: #8e182f;
  color: #f7cccc;
}

.filter-item.active {
  background: #8e182f;
  color: #f7cccc;
  font-weight: 600;
}

/* ── Content ── */
.content {
  flex: 1;
  overflow-y: auto;
  min-width: 0;
  background-color: var(--dark-bg);
}

/* ── Адаптив ── */
@media (max-width: 1280px) {
  .header {
    gap: 80px;
    padding: 0 16px;
  }

  .search-bar {
    width: 500px;
  }

  .sidebar {
    width: 200px;
  }

  .filter-title {
    font-size: 22px;
  }

  .filter-item {
    font-size: 18px;
  }

  .logo-img {
    width: 160px;
    height: 65px;
  }
}
</style>
