# Book2Screen

> **Система відгуків і рейтингів книг та їх екранізацій** — порівняння книги та фільму/серіалу на основі відгуків і рейтингів користувачів.

Клієнтська частина (Frontend) проєкту Book2Screen.

---

## Про проєкт

Book2Screen — це платформа, де користувачі можуть:
- Порівнювати книжки з їхніми кіноекранізаціями
- Читати й писати відгуки з можливістю позначення спойлерів
- Голосувати, що краще — книга чи її екранізація
- Досліджувати розбіжності сюжетів через **Інтерактивну карту відмінностей**

### Killer Feature: Інтерактивна карта відмінностей

На сторінці твору користувач бачить візуальне порівняння сюжету книги та фільму: зліва — події книги, справа — події фільму/серіалу, а між ними лінії, що демонструють змінені сцени, вирізані епізоди або додані сцени.

---

## Ключові можливості MVP

- **Реєстрація та авторизація** — створення акаунту, вхід, відновлення паролю (JWT)
- **Пошук і фільтри** — за жанром, країною, роком, назвою
- **Обране** — збереження улюблених творів у профілі користувача
- **Голосування «Книга vs Фільм»** — з живим прогрес-баром результатів
- **Відгуки зі спойлерами** — автоматичне блюрення тексту, позначеного як спойлер
- **Інтерактивна карта відмінностей** — візуальне порівняння сюжетів

---

## Технологічний стек

### Frontend
| Технологія | Версія | Призначення |
|------------|--------|-------------|
| Vue 3 (Composition API) | 3.5.31 | UI Framework |
| TypeScript | 5.9.3 | Статична типізація |
| Vite | 8.0.3 | Dev-сервер + збірка |
| Vue Router | 4.6.4 | SPA-навігація |
| Pinia | 3.0.4 | State Management |
| Axios | 1.15.0 | HTTP-клієнт з JWT |
| ESLint + Prettier | 10.1 / 3.8 | Лінтинг і форматування |

### Backend (окремий репозиторій)
C# / ASP.NET Core + PostgreSQL + Entity Framework Core

---

## Getting Started

### Prerequisites

- **Node.js** ≥ 20.x ([завантажити](https://nodejs.org/))
- **npm** ≥ 10.x (встановлюється разом із Node.js)
- Запущений backend-сервер (див. репозиторій backend-команди)

### Installation

1. Клонувати репозиторій:
   ```bash
   git clone https://github.com/kazikSUPER/Book2Screen.git
   cd Book2Screen
   ```

2. Встановити залежності:
   ```bash
   npm install
   ```

3. Створити файл `.env` на основі шаблону:
   ```bash
   cp .env.example .env
   ```

4. У `.env` вказати URL backend-сервера:
   ```
   VITE_API_URL=http://localhost:5000
   ```

### Running

```bash
# Dev-сервер (з hot reload)
npm run dev

# Production build
npm run build

# Preview production build
npm run preview
```

Після запуску `npm run dev` застосунок буде доступний на `http://localhost:5173`.

---

## NPM Scripts

| Команда | Що робить |
|---------|-----------|
| `npm run dev` | Запуск dev-сервера з HMR |
| `npm run build` | Production build (TypeScript check + Vite) |
| `npm run preview` | Локальний запуск production-білда |
| `npm run lint` | Перевірка коду через ESLint |
| `npm run lint:fix` | Автоматичне виправлення ESLint-помилок |
| `npm run format` | Форматування коду через Prettier |
| `npm run format:check` | Перевірка форматування (без змін) |

---

## Структура проєкту

```
Book2Screen/
├── docs/                      # Внутрішня документація
│   ├── naming-conventions.md  # Правила іменування
│   └── tech-stack.md          # Обґрунтування стеку
├── public/                    # Статичні ресурси (favicon тощо)
├── src/
│   ├── assets/                # Зображення, SVG
│   ├── components/            # UI-компоненти (модальні вікна тощо)
│   │   ├── LoginModal.vue
│   │   ├── RegisterModal.vue
│   │   └── ResetPasswordModal.vue
│   ├── views/                 # Сторінки (маршрути)
│   │   ├── HomeView.vue
│   │   ├── DetailView.vue     # Сторінка порівняння книги/фільму
│   │   ├── SearchView.vue
│   │   └── LoginView.vue
│   ├── services/              # API-інтеграція
│   │   ├── api.ts             # Axios клієнт + JWT interceptor
│   │   └── items.ts           # Запити ресурсів
│   ├── state/                 # Pinia stores
│   │   └── user.ts            # Auth state (JWT, email, isAuthenticated)
│   ├── hooks/                 # Composables (use-функції)
│   │   └── useFilter.ts       # Логіка фільтрації
│   ├── router/                # Конфігурація Vue Router
│   │   └── index.ts
│   ├── App.vue                # Root-компонент (layout)
│   ├── main.ts                # Entry point
│   └── style.css              # Глобальні стилі
├── .editorconfig              # Універсальні правила форматування
├── .env.example               # Шаблон змінних середовища
├── eslint.config.js           # ESLint Flat Config
├── .prettierrc                # Налаштування Prettier
├── .github/
│   └── pull_request_template.md  # PR-шаблон
└── vite.config.ts
```

Детальний опис архітектури — у документі *4.2.3 SAD: Internal View* у [Project Hub](https://www.notion.so/Project-Hub-Book2Screen-322bbedd49dc80b48c0ad04db0152497).

---

## Архітектура

Фронтенд побудований за принципами **компонентної архітектури з чітким поділом відповідальності**, що повторює Clean Architecture-підхід backend-частини:

| Шар | Папка | Відповідальність |
|-----|-------|------------------|
| **Pages** | `views/` | Сторінки, прив'язані до маршрутів |
| **Presentation** | `components/` | Перевикористовувані UI-компоненти |
| **Services** | `services/` | Інтеграція з Backend API |
| **State** | `state/` | Глобальний стан (Pinia) |
| **Hooks** | `hooks/` | Перевикористовувана логіка |
| **Routing** | `router/` | Маршрутизація SPA |

**Авторизація:** Stateless JWT. Токен зберігається в Pinia store і автоматично додається до кожного HTTP-запиту через axios interceptor.

---

## Команда

| Роль | Ім'я |
|------|------|
| **Project Manager** | Урсуляк Олександра |
| **Backend Developer** | Казімір Віталій |
| **Frontend Developer** | Андрющенко Людмила |
| **QA Engineer** | Костецька Христина |
| **Database Developer** | Іщук Єгор |

---

## Документація та артефакти

- **Project Hub:** [Notion](https://www.notion.so/Project-Hub-Book2Screen-322bbedd49dc80b48c0ad04db0152497)
- **Архітектурні рішення (ADR, SAD, Data Flow):** розділ *4.2 Architecture & Tech Stack* у Project Hub
- **UI/UX Prototype:** розділ *3.2 UI/UX Prototype* у Project Hub
- **Test Plan & Cases:** розділ *5.1 Test Plan & Strategy* у Project Hub
- **Naming Conventions:** [`docs/naming-conventions.md`](./docs/naming-conventions.md)
- **Tech Stack (детально):** [`docs/tech-stack.md`](./docs/tech-stack.md)

---

##Процес розробки

- **Code Style:** ESLint + Prettier з автоматичним форматуванням при збереженні (Format on Save).
- **Naming Conventions:** див. [`docs/naming-conventions.md`](./docs/naming-conventions.md).
- **Pull Requests:** використовується шаблон [`pull_request_template.md`](./.github/pull_request_template.md) з обов'язковим Self-Review Checklist.
- **Заборонені назви змінних:** `data`, `info`, `temp` (контролюється через ESLint правило `id-denylist`).
