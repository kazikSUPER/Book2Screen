# Naming Conventions — Book2Screen (Frontend)

Цей документ фіксує правила іменування, форматування та документування коду у фронтенд-частині проєкту **Book2Screen** (Vue 3 + TypeScript + Vite). Мета — забезпечити єдиний стиль написання коду всередині команди та уникнути «людського фактору» при рев'ю.

---

## 1. Структура та іменування файлів і папок

### 1.1. Папки

- Використовується **camelCase** для папок усередині `src/`.
- Назва папки має відображати її функціональне призначення (а не тип файлів усередині).

**Приклади:**

```
src/
├── components/   # дрібні перевикористовувані UI-компоненти
├── views/        # сторінки (маршрути)
├── services/     # модулі взаємодії з Backend API
├── state/        # глобальний стан (Pinia stores)
├── hooks/        # composables (Vue 3 hooks)
├── router/       # конфігурація vue-router
└── assets/       # статичні ресурси (зображення, SVG)
```

### 1.2. Vue-компоненти (`.vue`)

- Файли компонентів — у форматі **PascalCase**.
- Назва файлу має збігатися з назвою основного компонента всередині.
- Для модальних вікон використовується суфікс `Modal`, для сторінок — суфікс `View`.

**Правильно:**

```
LoginModal.vue
RegisterModal.vue
HomeView.vue
DetailView.vue
```

**Неправильно:**

```
login-modal.vue   
home.vue          
```

### 1.3. TypeScript-файли (`.ts`)

- Сервіси, хуки (composables), утиліти, store-и — у форматі **camelCase**.
- Назва файлу має починатися з дієслова або відображати сутність.

**Правильно:**

```
api.ts
items.ts
useFilter.ts
user.ts
```

**Неправильно:**

```
API.ts          
use-filter.ts   
UserStore.ts    
```

### 1.4. Композиції (Vue Composables / Hooks)

- Назва файлу та функції починається з префікса **`use`**.
- Приклад: `useFilter.ts` → `export function useFilter(...)`.

### 1.5. Статичні ресурси (`assets/`)

- Зображення та SVG — **осмислена назва** у **kebab-case** або **camelCase**.
- Заборонені однобуквенні або беззмістовні назви (`t.svg`, `img1.png`).

**Правильно:**

```
hero-banner.png
search-icon.svg
```

---

## 2. Іменування всередині коду

### 2.1. Класи, інтерфейси, типи

- **PascalCase**.
- Інтерфейси, що описують структуру даних з API, мають суфікс `DTO` або `Response`/`Request`.

```ts
interface BookScreenItem { ... }
interface LoginRequest { ... }
interface LoginResponse { ... }
interface ApiError { ... }
```

### 2.2. Функції та змінні

- **camelCase**.

```ts
const filteredItems = computed(...);
function login(userEmail: string, userToken: string) { ... }
```

### 2.3. Константи

- **UPPER_SNAKE_CASE** — для «магічних» констант (конфіги, незмінні списки).

```ts
export const ALL_ITEMS: BookScreenItem[] = [...];
const MAX_LOGIN_ATTEMPTS = 5;
```

### 2.4. Boolean-змінні

- Починаються з префіксів **`is`**, **`has`**, **`can`**, **`should`**.

```ts
const isAuthenticated = ref(false);
const hasAccess = ...;
const canEdit = ...;
```

### 2.5. Події Vue (`emit`)

- **kebab-case** для назв подій (згідно зі style guide Vue).

```ts
emit('close');
emit('login-success', userData);
```

### 2.6. Props компонентів

- У коді — **camelCase**.
- У шаблоні (template) — **kebab-case**.

```ts
// У компоненті:
defineProps<{ userEmail: string }>();

// При використанні:
<LoginModal user-email="test@mail.com" />
```

### 2.7. Абревіатури

- В `PascalCase` — тільки перша літера велика: `GetUi`, а не `GetUI`.
- В `camelCase` — повністю малими: `getUi`.

```ts
// Правильно:
const apiUrl = ...;
interface HttpConfig { ... }

// Неправильно:
const APIUrl = ...;     
interface HTTPConfig { ... }   
```

---

## 3. Заборонені назви («сміттєві»)

Офіційно заборонено використовувати у коді наступні назви змінних та файлів:

- `data`
- `info`
- `temp`

Заборона контролюється автоматично через ESLint:

```json
"rules": {
  "id-denylist": ["error", "data", "info", "temp"]
}
```

**Замість них використовуй осмислені назви:**

| Замість | Використовуй |
|---------|--------------|
| `data`  | `items`, `products`, `userList`, `responseBody` |
| `info`  | `userDetails`, `meta`, `summary` |
| `temp`  | `draft`, `buffer`, `cached` |

---

## 4. Форматування коду

Правила форматування автоматично контролюються **Prettier** (`.prettierrc`) та **EditorConfig** (`.editorconfig`).

| Параметр | Значення |
|----------|----------|
| Відступ | 2 пробіли |
| Кодування | UTF-8 |
| Кінець рядка | LF |
| Максимальна довжина рядка | 120 символів |
| Крапка з комою в кінці | Обов'язково (`semi: true`) |
| Лапки для рядків | Одинарні (`'...'`) |
| Trailing comma | `es5` |

---

## 5. Коментарі та документація

- Публічні функції в сервісах (`src/services/`) та хуках (`src/hooks/`) — документуються через **JSDoc**.
- Inline-коментарі — лише коли код неочевидний (пояснюємо **чому**, а не **що**).

**Приклад:**

```ts
/**
 * Фільтрує список елементів за пошуковим запитом, жанром та країною.
 * @param items - повний список елементів
 * @param searchQuery - реактивний пошуковий запит
 * @returns об'єкт з computed-полем filteredItems
 */
export function useFilter(items: BookScreenItem[], searchQuery: Ref<string>, ...) {
  ...
}
```

---

## 6. Контроль якості

- Всі правила автоматично перевіряються через **ESLint** (`.eslintrc.json`) та **Prettier** (`.prettierrc`).
- IDE налаштовано на **Format on Save** — код форматується автоматично при збереженні файлу.
- Порушення стилю блокуються на етапі **Pull Request Code Review** (див. `.github/pull_request_template.md`).
