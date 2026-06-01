<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { RouterView, RouterLink, useRouter, useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import LoginModal from './components/LoginModal.vue';
import RegisterModal from './components/RegisterModal.vue';
import ResetPasswordModal from './components/ResetPasswordModal.vue';
import ToastContainer from './components/ToastContainer.vue';
import FilterPanel, { type FilterSection } from './components/FilterPanel.vue';
import Logo from './components/Logo.vue';
import IconUser from './components/IconUser.vue';
import { checkHealth } from './services/health';
import { useUserStore } from './state/user';
import { useFiltersStore } from './state/filters';
import { useWishlistStore } from './state/wishlist';

const router = useRouter();
const route = useRoute();
const userStore = useUserStore();
const filtersStore = useFiltersStore();
const wishlistStore = useWishlistStore();

// storeToRefs — стандартний спосіб отримати реактивні значення з Pinia store у компоненті.
// Без нього email/token — объєкт Ref, а не рядок, і includes() повертає false.
const { email, isAuthenticated } = storeToRefs(userStore);

// Адмін: залогінений + email містить 'admin'
const isAdmin = computed(() => isAuthenticated.value && email.value?.includes('admin'));

// ── Filter panel — показується тільки на сторінках, де є каталог.
// Конфігурація секцій залежить від маршруту (за дизайном Figma).
const filterPanelConfig = computed<{ visible: boolean; sections: FilterSection[] }>(() => {
  switch (route.name) {
    case 'home':
      // Home — за Figma тільки жанри і країни.
      return { visible: true, sections: ['genres', 'countries'] };
    case 'search':
      // Search — повний набір.
      return { visible: true, sections: ['sort', 'genres', 'countries', 'years', 'rating'] };
    default:
      // Top, Detail, Profile, Admin — без бічної панелі.
      return { visible: false, sections: [] };
  }
});

// ── Health-check при старті застосунку ──────────────
const backendStatus = ref<'checking' | 'up' | 'down'>('checking');

onMounted(async () => {
  try {
    await checkHealth();
    backendStatus.value = 'up';
    console.info('[Book2Screen] Backend /health: OK');

    if (userStore.isAuthenticated) {
      await wishlistStore.syncWithBackend();
    }
  } catch (err) {
    backendStatus.value = 'down';
    console.warn('[Book2Screen] Backend /health: FAILED', err);
  }
});

// ── Search ──────────────────────────────────────────
// Поле пошуку у хедері тепер пише напряму у filters store —
// тому будь-яка сторінка одразу бачить актуальний searchQuery.
const onSearchSubmit = () => {
  if (filtersStore.searchQuery.trim()) {
    router.push({ name: 'search', query: { q: filtersStore.searchQuery.trim() } });
  }
};

// ── Modals ──────────────────────────────────────────
type ModalType = 'login' | 'register' | 'reset' | null;
const activeModal = ref<ModalType>(null);

// ── Mobile filters drawer ──────────────────────────
// На мобільному фільтри ховаються у "шторку". Кнопка біля пошуку — її відкриває.
const isMobileFiltersOpen = ref(false);

// ── Auth ─────────────────────────────────────────────
const handleAuthClick = () => {
  if (isAuthenticated.value) {
    // Залогінений → переходить у профіль
    router.push({ name: 'profile' });
  } else {
    activeModal.value = 'login';
  }
};
</script>

<template>
  <div class="app-wrapper">
    <!-- Банер статусу бекенду -->
    <div v-if="backendStatus === 'down'" class="backend-status-banner">
      ⚠ Backend недоступний. Дані не завантажуються.
    </div>

    <header class="header">
      <RouterLink to="/" class="logo-link" aria-label="Book2Screen — на головну">
        <Logo size="md" />
      </RouterLink>

      <!-- Навігаційні кнопки -->
      <nav class="header-nav" aria-label="Основна навігація">
        <RouterLink to="/" class="nav-link" active-class="nav-link--active" exact>Головна</RouterLink>
        <RouterLink to="/top" class="nav-link" active-class="nav-link--active">ТОП</RouterLink>
        <RouterLink
          v-if="isAuthenticated"
          to="/profile"
          class="nav-link"
          active-class="nav-link--active"
        >Профіль</RouterLink>
        <RouterLink
          v-if="isAdmin"
          to="/admin"
          class="nav-link nav-link--admin"
          active-class="nav-link--active"
        >Адмін</RouterLink>
      </nav>

      <div class="search-bar header-search">
        <input v-model="filtersStore.searchQuery" type="text" placeholder="Пошук..." @keyup.enter="onSearchSubmit" />
        <button class="search-submit" aria-label="Шукати" @click="onSearchSubmit">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" />
            <path d="M21 21L16.5 16.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
        </button>
      </div>

      <button
        class="login-btn"
        :title="isAuthenticated ? `Профіль (${email})` : 'Увійти'"
        :aria-label="isAuthenticated ? 'Перейти в профіль' : 'Увійти'"
        @click="handleAuthClick"
      >
        <IconUser :size="28" />
      </button>
    </header>

    <!-- Mobile-only search-bar -->
    <div class="mobile-search">
      <div class="search-bar">
        <input v-model="filtersStore.searchQuery" type="text" placeholder="Пошук..." @keyup.enter="onSearchSubmit" />
        <span class="search-icon">🔍</span>
      </div>
      <!-- Mobile-кнопка для відкриття фільтрів -->
      <button class="mobile-filters-btn" @click="isMobileFiltersOpen = true">⚙ Фільтри</button>
    </div>

    <!-- Mobile-only nav bar (під рядком пошуку) -->
    <nav class="mobile-nav" aria-label="Мобільна навігація">
      <RouterLink to="/" class="mobile-nav__link" active-class="mobile-nav__link--active" exact>🏠 Головна</RouterLink>
      <RouterLink to="/top" class="mobile-nav__link" active-class="mobile-nav__link--active">🏆 ТОП</RouterLink>
      <RouterLink
        v-if="isAuthenticated"
        to="/profile"
        class="mobile-nav__link"
        active-class="mobile-nav__link--active"
      >👤 Профіль</RouterLink>
      <RouterLink
        v-if="isAdmin"
        to="/admin"
        class="mobile-nav__link mobile-nav__link--admin"
        active-class="mobile-nav__link--active"
      >⚙ Адмін</RouterLink>
      <button
        v-if="!isAuthenticated"
        class="mobile-nav__link mobile-nav__link--login"
        @click="activeModal = 'login'"
      >↩ Увійти</button>
    </nav>

    <div class="main-layout">
      <FilterPanel
        v-if="filterPanelConfig.visible"
        :sections="filterPanelConfig.sections"
        :mobile-open="isMobileFiltersOpen"
        @close-mobile="isMobileFiltersOpen = false"
      />

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

    <ToastContainer />
  </div>
</template>

<style>
/* :root токени винесено у src/style.css — глобально для всього застосунку. */

.app-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

/* ── Backend status banner ── */
.backend-status-banner {
  background: var(--text-error);
  color: white;
  text-align: center;
  padding: 8px;
  font-family: var(--font-display);
  font-size: 14px;
}

/* ── Header ── */
.header {
  background-color: var(--color-header);
  height: var(--header-height);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  gap: 16px;
  position: relative;
  flex-shrink: 0;
  border-bottom: 1px solid var(--color-header);
}

/* ── Header nav ── */
.header-nav {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

.nav-link {
  color: var(--text-on-dark);
  text-decoration: none;
  font-family: var(--font-display);
  font-size: 14px;
  padding: 6px 14px;
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  transition:
    background 0.2s,
    border-color 0.2s,
    color 0.2s;
  white-space: nowrap;
}

.nav-link:hover {
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.2);
}

.nav-link--active {
  background: var(--color-primary);
  border-color: var(--color-primary-dark);
  color: var(--text-on-primary);
}

.nav-link--admin {
  color: #ffd700;
  border-color: rgba(255, 215, 0, 0.3);
}

.nav-link--admin:hover {
  background: rgba(255, 215, 0, 0.15);
  border-color: rgba(255, 215, 0, 0.5);
}

.nav-link--admin.nav-link--active {
  background: #b8860b;
  border-color: #8b6914;
  color: #fff;
}

.logo-link {
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  flex-shrink: 0;
  padding: 4px 6px;
}

.search-bar {
  width: 700px;
  max-width: 100%;
  flex-shrink: 1;
  position: relative;
  display: flex;
  align-items: center;
}

.search-bar input {
  width: 100%;
  padding: 9px 40px 9px 16px;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-input);
  background-color: var(--color-input-bg);
  color: var(--text-on-light);
  font-size: 14px;
  font-family: var(--font-body);
  outline: none;
}

.search-bar input::placeholder {
  color: var(--text-muted);
}

.search-bar input:focus {
  border-color: var(--color-primary);
}

.search-submit {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  background: transparent;
  border: none;
  color: var(--text-on-light);
  cursor: pointer;
  padding: 4px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.search-submit:hover {
  color: var(--color-primary);
}

.login-btn {
  color: var(--text-on-dark);
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px 12px;
  flex-shrink: 0;
  line-height: 0;
  display: inline-flex;
  align-items: center;
}

.login-btn:hover {
  color: var(--color-panel-box);
}

/* ── Layout ── */
.main-layout {
  display: flex;
  flex: 1;
  gap: 16px;
  padding: 16px;
  min-height: 0;
  background-color: var(--color-page);
}

.content {
  flex: 1;
  overflow-y: auto;
  min-width: 0;
}

/* ── Mobile-only search-bar (під хедером, видно тільки ≤768px) ── */
.mobile-search {
  display: none;
  background-color: var(--color-header);
  padding: 8px 12px;
  flex-shrink: 0;
  gap: 8px;
  align-items: center;
}

.mobile-search .search-bar {
  width: 100%;
}

.mobile-filters-btn {
  background: var(--color-input-bg);
  color: var(--color-header);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  padding: 8px 12px;
  font-family: var(--font-display);
  font-size: 13px;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
}

/* ── Mobile nav bar (bottom strip, видно тільки ≤768px) ── */
.mobile-nav {
  display: none;
  background-color: var(--color-header);
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  padding: 6px 8px;
  flex-shrink: 0;
  gap: 4px;
  align-items: center;
  overflow-x: auto;
  scrollbar-width: none;
}

.mobile-nav::-webkit-scrollbar {
  display: none;
}

.mobile-nav__link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: var(--text-on-dark);
  text-decoration: none;
  font-family: var(--font-display);
  font-size: 13px;
  padding: 6px 12px;
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  white-space: nowrap;
  flex-shrink: 0;
  background: none;
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s;
}

.mobile-nav__link:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.15);
}

.mobile-nav__link--active {
  background: var(--color-primary) !important;
  border-color: var(--color-primary-dark) !important;
  color: var(--text-on-primary) !important;
}

.mobile-nav__link--admin {
  color: #ffd700;
}

.mobile-nav__link--login {
  color: var(--text-on-dark);
  opacity: 0.8;
}

/* ── Адаптив ── */
@media (max-width: 1280px) {
  .header {
    gap: 12px;
    padding: 0 16px;
  }
  .header-search {
    width: 400px;
  }
}

@media (max-width: 1024px) {
  .header {
    gap: 8px;
    padding: 0 12px;
  }
  .header-search {
    width: auto;
    flex: 1;
    max-width: 300px;
  }
  .nav-link {
    font-size: 13px;
    padding: 5px 10px;
  }
}

@media (max-width: 768px) {
  .header {
    gap: 8px;
    padding: 0 12px;
  }
  .header-search {
    display: none;
  }
  .header-nav {
    display: none; /* на мобільному навігацію виносимо в mobile-nav */
  }
  .mobile-search {
    display: flex;
  }
  .mobile-nav {
    display: flex;
  }
  .mobile-search .search-bar input {
    padding: 7px 36px 7px 14px;
    border-radius: var(--radius-sm);
  }
  .main-layout {
    padding: 8px;
    flex-direction: column;
    gap: 0;
  }
}

@media (max-width: 380px) {
  .header {
    height: 72px;
    padding: 0 12px;
  }
  .mobile-search {
    padding: 8px 10px;
  }
  .backend-status-banner {
    padding: 6px;
    font-size: 12px;
  }
}
</style>
