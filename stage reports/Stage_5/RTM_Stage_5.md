# Матриця простежуваності вимог (RTM) — Етап 5

Цей документ пов'язує бізнес-вимоги (Use Cases) з технічною реалізацією та тестами для підтвердження повної інтеграції системи.

## 📊 Матриця статусів інтеграції

| ID | Вимога (Use Case) | Код (Backend Implementation) | Тести (Verification) | Статус |
| :--- | :--- | :--- | :--- | :--- |
| **UC-01** | Реєстрація та Автентифікація | `AuthController`, `AuthService` | `AuthIntegrationTests`, `MappingAndAuthTests` | ✅ Integrated |
| **UC-02** | Система "Обране" (Wishlist) | `FavoritesController`, `FavoriteService` | `FavoriteServiceTests` | ✅ Integrated |
| **UC-03** | Пошук та фільтрація творів | `WorksController`, `WorkService` | `WorksIntegrationTests`, `WorkServiceTests` | ✅ Integrated |
| **UC-04** | Голосування (Книга vs Фільм) | `VotesController`, `VoteService` | `VoteServiceTests` | ✅ Integrated |
| **UC-05** | Відгуки та модерація | `ReviewsController`, `ReviewService`, `AdminReportsController` | `ReviewServiceTests`, `AdminTests` | ✅ Integrated |
| **SYS-01** | Глобальна обробка помилок | `GlobalExceptionHandler.cs` | `IntegrationTests` (Sanity check) | ✅ Integrated |
| **SYS-02** | Транзакційність (Atomicity) | `IDbContextTransaction` (Auth, Vote, Reviews) | `AuthService.Register`, `VoteService.VoteAsync` | ✅ Integrated |
| **SYS-03** | Валідація контрактів (DTO) | `ReviewRequestValidator`, `VoteValidator` | `ValidatorTests` | ✅ Integrated |

## 🔍 Технічні зауваження
*   Всі Use Cases покриті мінімум одним типом тестів (Unit або Integration).
*   Статус **Integrated** означає, що бекенд-ендпоінти готові до виклику з фронтенду, контракти синхронізовані, а дані зберігаються в PostgreSQL.
*   Для UC-05 реалізовано повний цикл: Користувач (Відгук) -> Користувач (Скарга) -> Адмін (Модерація).

*Дата оновлення: 2026-05-28*
