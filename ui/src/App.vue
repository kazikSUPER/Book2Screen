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
import IconLogout from './components/IconLogout.vue';
import { checkHealth } from './services/health';
import { useUserStore } from './state/user';
import { useFiltersStore } from './state/filters';
import { useWishlistStore } from './state/wishlist';
import { STR } from './constants';

const router = useRouter();
const route = useRoute();
const userStore = useUserStore();
const filtersStore = useFiltersStore();
const wishlistStore = useWishlistStore();
const t = STR.common;

// ── Filter panel — показується тільки на сторінках, де є каталог.
// За Figma: Home — Жанри+Країна (Frame 1), Search — повний набір.
const filterPanelConfig = computed<{ visible: boolean; sections: FilterSection[] }>(() => {
  switch (route.name) {
    case 'home':
      return { visible: true, sections: ['genres', 'countries'] };
    case 'search':
      return { visible: true, sections: ['sort', 'genres', 'countries', 'years', 'rating'] };
    default:
      // Top, Detail, Profile, Admin — без бічної панелі (за макетами).
      return { visible: false, sections: [] };
  }
});

// ── Health-check при старті ──────────────────────────
const backendStatus = ref<'checking' | 'up' | 'down'>('checking');

onMounted(async () => {
  try {
    await checkHealth();
    backendStatus.value = 'up';
    console.info('[Book2Screen] Backend OK');
  } catch (err) {
    backendStatus.value = 'down';
    console.warn('[Book2Screen] Backend FAILED', err);
  }
  // Якщо вже залогінені (токен у localStorage) — підтягуємо профіль і
  // синхронізуємо обране з бекенду.
  if (userStore.isAuthenticated) {
    userStore.refreshProfile().catch((err) => console.warn('[Book2Screen] refreshProfile failed (ignored):', err));
    wishlistStore.syncFromServer().catch((err) => console.warn('[Book2Screen] wishlist sync failed (ignored):', err));
  }
});

// ── Search ──────────────────────────────────────────
const onSearchSubmit = () => {
  if (filtersStore.searchQuery.trim()) {
    router.push({ name: 'search', query: { q: filtersStore.searchQuery.trim() } });
  }
};

// ── Modals ──────────────────────────────────────────
type ModalType = 'login' | 'register' | 'reset' | null;
const activeModal = ref<ModalType>(null);

// ── Mobile filters drawer ──────────────────────────
const isMobileFiltersOpen = ref(false);

// ── Auth: правий-верхній кут.
//  • не залогінений → іконка "людинка" → відкриває Login-модалку
//  • залогінений    → іконка "стрілка-вихід" → одразу logout + редірект на головну
//
// Перехід у профіль робиться через нав-ланку "Профіль" у хедері.
const handleAuthClick = () => {
  if (!userStore.isAuthenticated) {
    activeModal.value = 'login';
    return;
  }
  userStore.logout();
  // Якщо ми були на захищеній сторінці — стрибаємо на головну.
  if (route.meta.requiresAuth) {
    router.push({ name: 'home' });
  }
};

const authBtnTitle = computed(() =>
  userStore.isAuthenticated ? STR.auth.logoutTooltip(userStore.email) : STR.auth.loginTooltip
);
</script>

<template>
  <div class="app-wrapper">
    <!-- Банер статусу бекенду -->
    <div v-if="backendStatus === 'down'" class="backend-status-banner" role="status">⚠ {{ t.serverDown }}</div>

    <header class="header">
      <RouterLink to="/" class="logo-link" aria-label="Book2Screen — на головну">
        <Logo size="md" />
      </RouterLink>

      <!-- Основна навігація (Frame 10 / 143). Адмін-лінк видимий тільки адмінам. -->
      <nav class="main-nav" aria-label="Основна навігація">
        <RouterLink :to="{ name: 'home' }" class="main-nav__link" active-class="main-nav__link--active">
          Головна
        </RouterLink>
        <RouterLink :to="{ name: 'top' }" class="main-nav__link" active-class="main-nav__link--active">
          ТОП
        </RouterLink>
        <RouterLink
          v-if="userStore.isAuthenticated"
          :to="{ name: 'profile' }"
          class="main-nav__link"
          active-class="main-nav__link--active"
        >
          Профіль
        </RouterLink>
        <RouterLink
          v-if="userStore.isAdmin"
          :to="{ name: 'admin' }"
          class="main-nav__link main-nav__link--admin"
          active-class="main-nav__link--active"
        >
          Адмін-панель
        </RouterLink>
      </nav>

      <div class="search-bar header-search">
        <input
          v-model="filtersStore.searchQuery"
          type="search"
          :placeholder="t.search"
          :aria-label="t.search"
          @keyup.enter="onSearchSubmit"
        />
        <button class="search-submit" type="button" :aria-label="t.searchSubmit" @click="onSearchSubmit">
          <svg
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            aria-hidden="true"
          >
            <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" />
            <path d="M21 21L16.5 16.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
        </button>
      </div>

      <button
        class="login-btn"
        :class="{ 'login-btn--logout': userStore.isAuthenticated }"
        type="button"
        :title="authBtnTitle"
        :aria-label="authBtnTitle"
        @click="handleAuthClick"
      >
        <!-- Залогінений → стрілка виходу. Незалогінений → людинка (login). -->
        <IconLogout v-if="userStore.isAuthenticated" :size="28" />
        <IconUser v-else :size="28" />
      </button>
    </header>

    <!-- Mobile-only search-bar -->
    <div class="mobile-search">
      <div class="search-bar">
        <input
          v-model="filtersStore.searchQuery"
          type="search"
          :placeholder="t.search"
          :aria-label="t.search"
          @keyup.enter="onSearchSubmit"
        />
        <span class="search-icon" aria-hidden="true">🔍</span>
      </div>
      <button
        v-if="filterPanelConfig.visible"
        type="button"
        class="mobile-filters-btn"
        :aria-label="t.filters"
        @click="isMobileFiltersOpen = true"
      >
        {{ t.filters }}
      </button>
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

/* ── Header (за Figma: Лого + широкий пошук + іконка користувача) ── */
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
}

.logo-link {
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  flex-shrink: 0;
  padding: 0;
}

.logo-link:focus-visible {
  outline: 2px solid var(--color-panel-box);
  outline-offset: 2px;
}

.search-bar {
  /* Флекс-розтяг: заповнює простір між nav і іконкою юзера. */
  flex: 1;
  min-width: 200px;
  max-width: 100%;
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

/* Головна навігація хедера (Frame 10 / 143). */
.main-nav {
  display: inline-flex;
  align-items: center;
  gap: 24px;
  flex-shrink: 0;
}

.main-nav__link {
  color: var(--text-on-dark);
  font-family: var(--font-display);
  font-size: 16px;
  text-decoration: none;
  padding: 6px 4px;
  transition: color 0.15s;
}

.main-nav__link:hover {
  color: var(--color-panel-box);
}

.main-nav__link--active {
  color: var(--color-primary);
  font-weight: 600;
}

.main-nav__link:focus-visible {
  outline: 2px solid var(--color-panel-box);
  outline-offset: 2px;
  border-radius: var(--radius-xs);
}

/* Підсвічуємо "Адмін-панель" — щоб виділялась серед звичайних ланок. */
.main-nav__link--admin {
  background: var(--color-card);
  border: 1px solid var(--color-primary-dark);
  border-radius: var(--radius-sm);
  padding: 6px 14px;
}

.main-nav__link--admin:hover {
  background: var(--color-primary);
  color: var(--text-on-primary);
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

/* Коли в профілі — стрілка виходу в primary-кольорі, щоб підкреслити дію logout. */
.login-btn--logout {
  color: var(--color-primary);
}

.login-btn--logout:hover {
  color: var(--color-primary-hover);
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

@media (max-width: 1280px) {
  .header {
    gap: 16px;
    padding: 0 16px;
  }
}

@media (max-width: 1024px) {
  .header {
    gap: 12px;
    padding: 0 12px;
  }
}

@media (max-width: 1024px) {
  .main-nav {
    gap: 12px;
  }
  .main-nav__link {
    font-size: 14px;
  }
  .main-nav__link--admin {
    padding: 4px 10px;
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
  .main-nav {
    display: none; /* Ховаємо навігацію, щоб хедер був як на макеті (лише лого і логін) */
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
}
</style>
