import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import DetailView from '../views/DetailView.vue';
import SearchView from '../views/SearchView.vue';
import TopView from '../views/TopView.vue';
import ProfileView from '../views/ProfileView.vue';
import AdminView from '../views/AdminView.vue';
import NotFoundView from '../views/NotFoundView.vue';
import { useUserStore } from '../state/user';
import { useNotificationsStore } from '../state/notifications';

/**
 * Маршрутизація Book2Screen.
 *
 * meta.requiresAuth — потрібен будь-який залогінений юзер.
 * meta.requiresAdmin — додатково потрібен role 'admin' або 'moderator'.
 *
 * Без авторизації → редірект на головну з toast-попередженням.
 * Без admin-role → toast і редірект.
 * Невідомі URL → /:pathMatch(.*)* → NotFoundView (BUG-043).
 */
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'home', component: HomeView, meta: { title: 'Book2Screen' } },
    {
      path: '/search',
      name: 'search',
      component: SearchView,
      meta: { title: 'Пошук — Book2Screen' },
    },
    {
      path: '/item/:id',
      name: 'detail',
      component: DetailView,
      meta: { title: 'Деталі — Book2Screen' },
    },
    {
      path: '/top',
      name: 'top',
      component: TopView,
      meta: { title: 'Топ адаптацій — Book2Screen' },
    },
    {
      path: '/profile',
      name: 'profile',
      component: ProfileView,
      meta: { title: 'Профіль — Book2Screen', requiresAuth: true },
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      // BUG-038: тільки авторизовані з role 'admin'/'moderator'.
      meta: { title: 'Адмін-панель — Book2Screen', requiresAuth: true, requiresAdmin: true },
    },
    // BUG-043: кастомна 404 замість редіректу на головну.
    { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundView, meta: { title: '404 — Book2Screen' } },
  ],
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) return savedPosition;
    return { top: 0 };
  },
});

// ── Guard ─────────────────────────────────────────────
router.beforeEach((to: RouteLocationNormalized) => {
  const user = useUserStore();

  // 1. Авторизація потрібна?
  if (to.meta.requiresAuth && !user.isAuthenticated) {
    const notifications = useNotificationsStore();
    notifications.pushWarning('Увійдіть, щоб відкрити цю сторінку');
    return { name: 'home' };
  }

  // 2. Адмін-роль потрібна? (BUG-038)
  if (to.meta.requiresAdmin && !user.isAdmin) {
    const notifications = useNotificationsStore();
    notifications.pushError('Доступ тільки для адміністраторів');
    return { name: 'home' };
  }

  return true;
});

// Динамічний <title>.
router.afterEach((to) => {
  const title = (to.meta?.title as string | undefined) ?? 'Book2Screen';
  document.title = title;
});

export default router;
