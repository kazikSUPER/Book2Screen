# UI Traceability — User Stories ↔ Components

Документ зв'язує SCRUM-задачі (User Stories) з фронт-компонентами, де вони реалізовані.
Це частина Етапу 5, Крок 5 — **UI Traceability** і **Architecture Update**.

> Оновлюйте цей файл при додаванні нової фічі, щоб історія "що-де-чому" не губилась.

---

## SCRUM-67 — US 3.1 — Top Lists

**Опис:** Користувач переглядає найкращі / найгірші адаптації, фільтрує по
жанру, року, наявності карти відмінностей.

| Шар | Файл |
|-----|------|
| Сторінка | `src/views/TopView.vue` |
| Панель фільтрів | `src/components/TopFilterBar.vue` |
| Картка твору | `src/components/WorkCard.vue` |
| Стор фільтрів | `src/state/filters.ts` |
| Логіка фільтрації | `src/composables/useFilter.ts` |
| API | `src/services/works.ts` (fetchWorks) |

---

## SCRUM-68 — US 3.2 — Book Details

**Опис:** Сторінка деталей твору: дві картки порівняння (книга/екранізація),
зіркові оцінки, інтерактивна карта відмінностей, голосування і коментарі.

| Шар | Файл |
|-----|------|
| Сторінка | `src/views/DetailView.vue` |
| Карта відмінностей | `src/components/DifferencesMap.vue` |
| Зіркова оцінка | `src/components/StarRating.vue` |
| Wishlist стор | `src/state/wishlist.ts` |
| User-ratings стор | `src/state/userRatings.ts` |
| API | `src/services/works.ts` (fetchWorkById) |

---

## SCRUM-70 / SCRUM-71 — UC-04 — Voting

**Опис:** Користувач голосує "Книга" чи "Фільм" — і бачить розподіл відсотків.

| Шар | Файл |
|-----|------|
| Блок голосування | `src/components/VotingBlock.vue` |
| Стор власного голосу | `src/state/votes.ts` |
| API | `src/services/votes.ts` |
| Типи | `src/services/types.ts` (VoteRequest, VoteResponse) |

---

## SCRUM-72 — US 6.1 — Writing Review

**Опис:** Користувач залишає текстовий відгук, позначає спойлер, може
поскаржитись на чужий коментар.

| Шар | Файл |
|-----|------|
| Блок відгуків | `src/components/ReviewsBlock.vue` |
| Один відгук | `src/components/ReviewItem.vue` |
| Модалка скарги | `src/components/ReportCommentModal.vue` |
| API | `src/services/reviews.ts` |

---

## SCRUM-64 — US 1.3 — Personal Profile

**Опис:** Сторінка профілю користувача: аватар, інфо, статистика, мої оцінки,
мої відгуки, список "хочу переглянути/прочитати".

| Шар | Файл |
|-----|------|
| Сторінка | `src/views/ProfileView.vue` |
| Стор user-info | `src/state/user.ts` |
| API | `src/services/profile.ts` |

---

## SCRUM-143 — Admin Panel

**Опис:** Адмін-панель: CRUD книг + модерація коментарів + редактор карти
відмінностей.

| Шар | Файл |
|-----|------|
| Сторінка | `src/views/AdminView.vue` |
| API | `src/services/admin.ts` |

---

## US 1.1 / 1.2 — Auth: Register, Login, Password Reset

| Шар | Файл |
|-----|------|
| Login модалка | `src/components/LoginModal.vue` |
| Register модалка | `src/components/RegisterModal.vue` |
| Reset Password модалка | `src/components/ResetPasswordModal.vue` |
| Стор сесії | `src/state/user.ts` |
| API | `src/services/auth.ts` |

---

## SCRUM-66 — US 2.1 — Search Results

| Шар | Файл |
|-----|------|
| Сторінка | `src/views/SearchView.vue` |
| Поле пошуку в шапці | `src/App.vue` |
| Стор фільтрів | `src/state/filters.ts` |

---

## Загальна інфраструктура

| Підсистема | Файл |
|------------|------|
| Axios instance + interceptor | `src/services/api.ts` |
| Прапори середовища (USE_MOCK_FALLBACK) | `src/services/env.ts` |
| Витяг error message | `src/services/error.ts` |
| Health-check | `src/services/health.ts` |
| Локалізація (тексти UI) | `src/constants/strings.ts` |
| Каталожні довідники (жанри/країни) | `src/constants/catalog.ts` |
| Дизайн-токени (CSS-змінні) | `src/style.css` |
| Тост-нотифікації | `src/state/notifications.ts` + `src/components/ToastContainer.vue` |
| Persistence (localStorage) | `src/composables/usePersistedRef.ts` |
| Router + guards | `src/router/index.ts` |

---

## Структура папок (Architecture Update — Етап 5)

```
src/
├── App.vue                 ─ shell застосунку: шапка, навігація, layout
├── main.ts                 ─ створення app, реєстрація Pinia/Router
├── style.css               ─ глобальні CSS-змінні + reset + a11y базис
│
├── assets/                 ─ статика (Hero.png, svg-логотипи)
├── public/                 ─ публічні файли (favicon)
│
├── components/             ─ перевикористовувані компоненти UI
│   ├── DifferencesMap.vue
│   ├── FilterPanel.vue
│   ├── IconUser.vue
│   ├── LoginModal.vue
│   ├── Logo.vue
│   ├── RegisterModal.vue
│   ├── ReportCommentModal.vue
│   ├── ResetPasswordModal.vue
│   ├── ReviewItem.vue
│   ├── ReviewsBlock.vue
│   ├── StarRating.vue
│   ├── ToastContainer.vue
│   ├── TopFilterBar.vue
│   ├── VotingBlock.vue
│   └── WorkCard.vue
│
├── views/                  ─ сторінки (бекенд маршрутів)
│   ├── AdminView.vue
│   ├── DetailView.vue
│   ├── HomeView.vue
│   ├── ProfileView.vue
│   ├── SearchView.vue
│   └── TopView.vue
│
├── router/                 ─ vue-router (маршрути + guards)
│   └── index.ts
│
├── state/                  ─ Pinia store'и (Global Store, Етап 5)
│   ├── filters.ts          ─ глобальний фільтр каталогу
│   ├── notifications.ts    ─ глобальні тости
│   ├── user.ts             ─ сесія і профіль
│   ├── userRatings.ts      ─ мої зіркові оцінки
│   ├── votes.ts            ─ мої голоси Книга/Фільм
│   ├── wishlist.ts         ─ "Хочу прочитати/переглянути"
│   └── favorites.ts        ─ DEPRECATED (re-export wishlist)
│
├── composables/            ─ Vue 3 composables (use*)
│   ├── useFilter.ts        ─ клієнтська фільтрація + сортування
│   └── usePersistedRef.ts  ─ persisted ref у localStorage
│
├── hooks/                  ─ DEPRECATED (re-export з composables)
│   └── useFilter.ts
│
├── services/               ─ HTTP-доступ до бекенду
│   ├── admin.ts
│   ├── api.ts              ─ axios instance + interceptor'и
│   ├── auth.ts
│   ├── env.ts              ─ читання env-прапорів
│   ├── error.ts            ─ extractErrorMessage()
│   ├── health.ts
│   ├── items.ts            ─ mock-дані для DEV
│   ├── profile.ts
│   ├── reviews.ts
│   ├── types.ts            ─ доменні + DTO інтерфейси
│   ├── votes.ts
│   └── works.ts
│
└── constants/              ─ UI-тексти, довідники
    ├── catalog.ts          ─ GENRES, COUNTRIES, SORT_OPTIONS, REPORT_REASONS
    ├── strings.ts          ─ STR.{common, auth, home, top, detail, profile, admin, report, search}
    └── index.ts            ─ barrel re-export
```

**Принципи:**

1. **Один напрямок даних:** View → composables → store → service → API.
2. **Stateless components, stateful stores:** ніяких глобальних змінних поза Pinia.
3. **Zero-Mocks Policy** (Етап 2): моки тільки коли `VITE_USE_MOCK_FALLBACK=true`.
4. **Persistence через usePersistedRef** — synced з localStorage прозоро.
5. **Локалізація централізована** — усі видимі тексти у `src/constants/strings.ts`.
