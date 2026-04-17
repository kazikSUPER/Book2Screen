<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, RouterLink } from 'vue-router'

const genres: string[] = [
  'Комедія', 'Драма', 'Фантастика', 'Фентезі',
  'Жахи', 'Детектив', 'Кримінал', 'Пригоди',
  'Історичні', 'Біографічні', 'Документальні',
]

const countries: string[] = [
  'Україна', 'США', 'Велика Британія', 'Канада',
  'Греція', 'Італія', 'Туреччина', 'Іспанія',
  'Німеччина', 'Японія', 'Швеція',
]

const selectedGenre = ref<string | null>(null)
const selectedCountry = ref<string | null>(null)
const searchQuery = ref('')
</script>

<template>
  <div class="app-wrapper">
    <header class="header">
      <RouterLink to="/" class="logo">
        Book 📖<br />+<br />🎬 Screen
      </RouterLink>

      <div class="search-bar">
        <input v-model="searchQuery" type="text" placeholder="Пошук..." />
        <span class="search-icon">🔍</span>
      </div>

      <RouterLink to="/login" class="login-btn">→</RouterLink>
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
            >{{ genre }}</li>
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
            >{{ country }}</li>
          </ul>
        </div>
      </aside>

      <main class="content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style>
:root {
  --pink-light: #F7CCCC;
  --pink-mid:   #E4AFAF;
  --dark-bg:    #311620;
  --dark-card:  #3D0000;
  --accent:     #8E182F;
}

*, *::before, *::after { box-sizing: border-box; }

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

/* ── Header ── */
.header {
  background-color: var(--dark-card);
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  gap: 16px;
  flex-shrink: 0;
}

.logo {
  background-color: var(--pink-light);
  color: var(--dark-card);
  text-decoration: none;
  padding: 6px 14px;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 700;
  line-height: 1.4;
  text-align: center;
  white-space: nowrap;
  flex-shrink: 0;
}

.search-bar {
  flex: 1;
  max-width: 520px;
  position: relative;
  display: flex;
  align-items: center;
}

.search-bar input {
  width: 100%;
  padding: 9px 40px 9px 16px;
  border-radius: 20px;
  border: none;
  background-color: var(--pink-light);
  color: var(--dark-card);
  font-size: 14px;
  font-family: 'Georgia', serif;
  outline: none;
}

.search-bar input::placeholder { color: var(--accent); }

.search-icon {
  position: absolute;
  right: 12px;
  font-size: 16px;
  pointer-events: none;
}

.login-btn {
  color: var(--pink-light);
  text-decoration: none;
  font-size: 26px;
  flex-shrink: 0;
  transition: color 0.2s;
}
.login-btn:hover { color: #fff; }

/* ── Layout ── */
.main-layout {
  display: flex;
  flex: 1;
}

/* ── Sidebar ── */
.sidebar {
  width: 185px;
  flex-shrink: 0;
  background-color: var(--pink-light);
  border-right: 2px solid var(--dark-card);
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
}

.filter-box {
  background-color: var(--pink-mid);
  border: 1px solid var(--dark-card);
  border-radius: 6px;
  padding: 10px 12px;
}

.filter-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--dark-card);
  margin: 0 0 8px;
  text-align: center;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--dark-card);
}

.filter-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.filter-item {
  font-size: 13px;
  color: var(--dark-card);
  cursor: pointer;
  padding: 3px 6px;
  border-radius: 4px;
  transition: background 0.15s, color 0.15s;
}

.filter-item:hover {
  background: var(--accent);
  color: var(--pink-light);
}

.filter-item.active {
  background: var(--accent);
  color: var(--pink-light);
  font-weight: 600;
}

/* ── Content ── */
.content {
  flex: 1;
  overflow-y: auto;
  min-width: 0;
  background-color: var(--dark-bg);
}
</style>
