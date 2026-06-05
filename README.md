# Book2Screen

> **Система відгуків і рейтингів книг та їх екранізацій** — порівняння книги та фільму/серіалу на основі відгуків і рейтингів користувачів.

Повнофункціональний веб-застосунок (Fullstack), що включає клієнтську частину на Vue.js та серверну частину на ASP.NET Core.

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

### Backend
| Технологія | Версія | Призначення |
|------------|--------|-------------|
| .NET 10 | 10.0 | Runtime |
| ASP.NET Core | 10.0 | Web Framework |
| Entity Framework Core | 10.0 | ORM |
| PostgreSQL | 15 | Database |
| Npgsql | 10.0 | Data Provider |
| AutoMapper | 16.1 | DTO Mapping |
| FluentValidation | 11.9 | Validation |
| StyleCop | 1.2 | Code Style |

---

## Getting Started

### Prerequisites

- **Node.js** ≥ 20.x ([завантажити](https://nodejs.org/))
- **npm** ≥ 10.x
- **.NET SDK 10.0** ([завантажити](https://dotnet.microsoft.com/download))
- **Docker & Docker Compose** (рекомендовано)

### Installation & Running (Docker - Quick Start)

Найпростіший спосіб запустити весь проєкт (Frontend + Backend + Database):

1. Клонувати репозиторій:
   ```bash
   git clone https://github.com/kazikSUPER/Book2Screen.git
   cd Book2Screen
   ```

2. Створити файл `.env` на основі шаблону:
   ```bash
   cp .env.example .env
   ```

3. Запустити через Docker Compose:
   ```bash
   docker-compose up --build
   ```

Після цього:
- **Frontend:** `http://localhost:3000`
- **Backend API:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger`
- **pgAdmin:** `http://localhost:5050`

### Manual Setup

#### Backend
1. Перейти до папки сервера: `cd server`
2. Відновити пакети: `dotnet restore`
3. Застосувати міграції: `dotnet ef database update`
4. Запустити: `dotnet run`

#### Frontend
1. Перейти до папки UI: `cd ui`
2. Встановити залежності: `npm install`
3. Запустити: `npm run dev`

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
├── server/                    # Backend (ASP.NET Core)
│   ├── API (Web)/             # Controllers, Middleware, Configs
│   ├── Application/           # Services, DTOs, Validators, Interfaces
│   ├── Domain/                # Entities, Custom Exceptions
│   └── Infrastructure/        # DB Context, Migrations, External Services
├── ui/                        # Frontend (Vue 3 + Vite)
│   ├── src/
│   │   ├── components/        # UI-компоненти
│   │   ├── views/             # Сторінки (маршрути)
│   │   ├── services/          # API-інтеграція
│   │   └── state/             # Pinia stores
│   └── ...
├── Book2Screen.Test/          # Unit & Integration Tests (xUnit)
├── docs/                      # Документація проєкту
├── compose.yaml               # Docker Compose конфігурація
└── README.md
```

Детальний опис архітектури — у документі *4.2.3 SAD: Internal View* у [Project Hub](https://www.notion.so/Project-Hub-Book2Screen-322bbedd49dc80b48c0ad04db0152497).

---

## Архітектура

Проєкт побудований на принципах **Clean Architecture** (Backend) та **Component-Based Architecture** (Frontend):

### Backend (Clean Architecture)
| Шар | Відповідальність |
|-----|------------------|
| **Domain** | Корневі сутності та бізнес-виключення |
| **Application** | Бізнес-логіка, DTO, валідатори, інтерфейси |
| **Infrastructure** | Робота з БД (EF Core), зовнішні сервіси (Email, Tokens) |
| **API (Web)** | Контролери, Middleware, конфігурація DI |

### Frontend
| Шар | Відповідальність |
|-----|------------------|
| **Pages (Views)** | Сторінки, прив'язані до маршрутів |
| **Presentation** | Перевикористовувані UI-компоненти |
| **Services** | Інтеграція з Backend API (Axios) |
| **State** | Глобальний стан (Pinia) |

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

## Процес розробки

- **Code Style:** ESLint + Prettier з автоматичним форматуванням при збереженні (Format on Save).
- **Naming Conventions:** див. [`docs/naming-conventions.md`](./docs/naming-conventions.md).
- **Pull Requests:** використовується шаблон [`pull_request_template.md`](./.github/pull_request_template.md) з обов'язковим Self-Review Checklist.
- **Заборонені назви змінних:** `data`, `info`, `temp` (контролюється через ESLint правило `id-denylist`).
