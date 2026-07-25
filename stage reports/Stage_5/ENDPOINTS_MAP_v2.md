# Повна карта ендпоїнтів (API Mapping) — v2 (Етап 5)

## 📊 Легенда
- ✅ **BE** — Реалізовано в ASP.NET Core контролері.
- ✅ **FE** — Використовується/Викликається у Vue.js сервісах.
- ❌ **BE** — Відсутній на бекенді.
- ❌ **FE** — Не викликається на фронтенді.

---

## 1. Автентифікація (`/api/v1/auth`)
| Ендпоїнт          | Метод | BE | FE | Frontend сервіс / Метод            | Коментар                   |
| :---------------- | :---: | :-: | :-: | :--------------------------------- | :------------------------- |
| `/login`          | POST  | ✅ | ✅ | `auth.ts -> login()`               | Вхід, повертає JWT.        |
| `/register`       | POST  | ✅ | ✅ | `auth.ts -> register()`            | Реєстрація нового користувача. |
| `/password-reset` | POST  | ✅ | ✅ | `auth.ts -> requestPasswordReset()` | Запит коду на Email.       |
| `/verify-code`    | POST  | ✅ | ✅ | `auth.ts -> verifyCode()`           | Перевірка коду.            |
| `/reset-password` | POST  | ✅ | ✅ | `auth.ts -> confirmPasswordReset()` | Встановлення нового пароля. |

## 2. Твори (`/api/v1/works`)
| Ендпоїнт   | Метод | BE | FE | Frontend сервіс / Метод    | Коментар                        |
| :--------- | :---: | :-: | :-: | :------------------------- | :------------------------------ |
| `/`        | GET   | ✅ | ✅ | `works.ts -> fetchWorks()` | Список творів з фільтрацією.    |
| `/top`     | GET   | ✅ | ✅ | `works.ts -> fetchTopWorks()` | Топ творів.                     |
| `/{id}`    | GET   | ✅ | ✅ | `works.ts -> fetchWorkById()` | Деталі твору (+ карта розбіжностей). |

## 3. Адміністрування (`/api/v1/admin`)
| Ендпоїнт                 | Метод | BE | FE | Frontend сервіс / Метод       | Коментар                        |
| :----------------------- | :---: | :-: | :-: | :---------------------------- | :------------------------------ |
| `/adaptations`           | POST  | ✅ | ✅ | `admin.ts -> createBook()`     | Створити нову адаптацію.        |
| `/adaptations/{id}`      | GET   | ✅ | ✅ | `admin.ts -> fetchWorkById()`  | Отримати для редагування.       |
| `/adaptations/{id}`      | PUT   | ✅ | ✅ | `admin.ts -> updateBook()`     | Оновити дані адаптації.         |
| `/adaptations/{id}`      | DELETE| ✅ | ✅ | `admin.ts -> deleteBook()`     | Видалити адаптацію.             |
| `/reports`               | GET   | ✅ | ✅ | `admin.ts -> fetchReports()`   | Список скарг.                   |
| `/reports/{id}/approve`  | POST  | ✅ | ✅ | `admin.ts -> moderateReport()` | Схвалити скаргу (видалити відгук). |
| `/reports/{id}/reject`   | POST  | ✅ | ✅ | `admin.ts -> moderateReport()` | Відхилити скаргу.               |
| `/reports/{id}/spoiler`  | POST  | ✅ | ✅ | `admin.ts -> moderateReport()` | Позначити як спойлер.           |

## 4. Обране (`/api/v1/favorites`)
| Ендпоїнт          | Метод | BE | FE | Frontend сервіс / Метод            | Коментар             |
| :---------------- | :---: | :-: | :-: | :--------------------------------- | :------------------- |
| `/`               | GET   | ✅ | ✅ | `favorites.ts -> fetchFavorites()` | Список обраного.     |
| `/`               | POST  | ✅ | ✅ | `favorites.ts -> addToFavorites()` | Додати в обране.     |
| `/{workId}`       | DELETE| ✅ | ✅ | `favorites.ts -> removeFromFavorites()` | Видалити з обраного. |
| `/check/{workId}` | GET   | ✅ | ✅ | `favorites.ts -> checkIsFavorite()` | Перевірити наявність. |

## 5. Відгуки (`/api/v1/reviews`)
| Ендпоїнт       | Метод | BE | FE | Frontend сервіс / Метод       | Коментар                   |
| :------------- | :---: | :-: | :-: | :---------------------------- | :------------------------- |
| `/work/{workId}`| GET   | ✅ | ✅ | `reviews.ts -> fetchReviews()` | Отримати відгуки до твору. |
| `/`            | POST  | ✅ | ✅ | `reviews.ts -> submitReview()` | Додати новий відгук.       |
| `/{id}`        | PUT   | ✅ | ✅ | `profile.ts -> updateMyReview()` | Редагувати свій відгук.    |
| `/{id}`        | DELETE| ✅ | ✅ | `profile.ts -> deleteMyReview()` | Видалити свій відгук.      |
| `/{id}/report` | POST  | ✅ | ✅ | `reviews.ts -> reportReview()` | Поскаржитись на відгук.    |

## 6. Голосування (`/api/v1/votes`)
| Ендпоїнт   | Метод | BE | FE | Frontend сервіс / Метод         | Коментар             |
| :--------- | :---: | :-: | :-: | :------------------------------ | :------------------- |
| `/{workId}`| GET   | ✅ | ✅ | `votes.ts -> fetchVoteResults()` | Статистика голосування. |
| `/`        | POST  | ✅ | ✅ | `votes.ts -> submitVote()`       | Проголосувати.       |

## 7. Користувачі (`/api/v1/users/me`)
| Ендпоїнт     | Метод | BE | FE | Frontend сервіс / Метод       | Коментар               |
| :----------- | :---: | :-: | :-: | :---------------------------- | :--------------------- |
| `/`          | GET   | ✅ | ✅ | `profile.ts -> fetchMyProfile()` | Дані профілю.          |
| `/`          | PUT   | ✅ | ✅ | `profile.ts -> updateMyProfile()` | Оновити дані профілю.  |
| `/avatar`    | POST  | ✅ | ✅ | `profile.ts -> updateAvatar()`   | Завантажити аватар.    |
| `/reviews`   | GET   | ✅ | ✅ | `profile.ts -> fetchMyReviews()` | Мої відгуки.           |

---

## 🔍 Підсумок (Етап 5)
Всі ендпоїнти повністю синхронізовані з фронтендом. Додано валідацію DTO на бекенді, що забезпечує стабільність інтеграції.

*Останнє оновлення: 2026-05-28*
