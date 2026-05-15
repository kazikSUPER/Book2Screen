import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import DetailView from '../views/DetailView.vue';
import SearchView from '../views/SearchView.vue';
import TopView from '../views/TopView.vue';
import ProfileView from '../views/ProfileView.vue';
import AdminView from '../views/AdminView.vue';

// LoginView видалено — авторизація іде через модалку (LoginModal),
// окрема сторінка /login була дублем і не використовувалась.
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/search',
      name: 'search',
      component: SearchView,
    },
    {
      path: '/item/:id',
      name: 'detail',
      component: DetailView,
    },
    {
      path: '/top',
      name: 'top',
      component: TopView,
    },
    {
      path: '/profile',
      name: 'profile',
      component: ProfileView,
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
    },
  ],
});

export default router;
