<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { RouterView, RouterLink, useRouter, useRoute } from 'vue-router';
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

const router = useRouter();
const route = useRoute();
const userStore = useUserStore();
const filtersStore = useFiltersStore();

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
  if (userStore.isAuthenticated) {
    userStore.logout();
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
        :title="userStore.isAuthenticated ? `Вийти (${userStore.email})` : 'Вхід'"
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
    <ResetPasswordModal v-if="activeModal === 'reset'" @close="activeModal = null" @success="activeModal = null" />

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
  gap: 24px;
  position: relative;
  flex-shrink: 0;
  border-bottom: 1px solid var(--color-header);
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

/* ── Адаптив ── */
@media (max-width: 1280px) {
  .header {
    gap: 16px;
    padding: 0 16px;
  }
  .header-search {
    width: 500px;
  }
}

@media (max-width: 1024px) {
  .header {
    gap: 12px;
    padding: 0 12px;
  }
  .header-search {
    width: auto;
    flex: 1;
    max-width: 400px;
  }
}

@media (max-width: 768px) {
  .header {
    gap: 12px;
    padding: 0 16px;
  }
  .header-search {
    display: none;
  }
  .mobile-search {
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
