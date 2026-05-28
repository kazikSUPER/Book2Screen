import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import DetailView from '../views/DetailView.vue';
import SearchView from '../views/SearchView.vue';
import TopView from '../views/TopView.vue';
import ProfileView from '../views/ProfileView.vue';
import AdminView from '../views/AdminView.vue';
import { useUserStore } from '../state/user';
import { useNotificationsStore } from '../state/notifications';

/**
 * Маршрутизація Book2Screen.
 *
 * Захищені маршрути (meta.requiresAuth) — Profile, Admin.
 * Якщо користувач не залогінений — редіректимо на головну і показуємо тост.
 * Решта — публічні: home, search, top, detail.
 *
 * LoginView видалено — авторизація іде через модалку (LoginModal),
 * окрема сторінка /login була дублем і не використовувалась.
 */
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: { title: 'Book2Screen' },
    },
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
      meta: { title: 'Адмін-панель — Book2Screen', requiresAuth: true },
    },
    // 404 — будь-який невідомий шлях кидаємо на головну.
    {
      path: '/:pathMatch(.*)*',
      redirect: { name: 'home' },
    },
  ],
  // Завжди скролимо нагору при зміні маршруту.
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) return savedPosition;
    return { top: 0 };
  },
});

// ── Guard для захищених маршрутів ─────────────────────
router.beforeEach((to: RouteLocationNormalized) => {
  if (to.meta.requiresAuth) {
    const user = useUserStore();
    if (!user.isAuthenticated) {
      const notifications = useNotificationsStore();
      notifications.pushWarning('Увійдіть, щоб відкрити цю сторінку');
      return { name: 'home' };
    }
  }
  return true;
});

// ── Динамічний <title> для accessibility / SEO ────────
router.afterEach((to) => {
  const title = (to.meta?.title as string | undefined) ?? 'Book2Screen';
  document.title = title;
});

export default router;
