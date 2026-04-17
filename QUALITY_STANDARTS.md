🎯 Стандарти якості та правила розробки (Quality Standards)

Цей документ є технічним регламентом проєкту Book2Screen. Він визначає єдині правила для всієї команди, забезпечуючи стабільність коду, прозорість розробки та відповідність вимогам ментора.

🏗 1. Стандарти коду (Code Style)

| Елемент | Стиль іменування | Приклад |
| :--- | :--- | :--- |
| **Проєкти / Solution Folders** | `PascalCase` | `Book2Screen.Infrastructure` |
| **Класи / Сутності / DTO** | `PascalCase` | `MovieEntity`, `UserDto` |
| **Інтерфейси** | `IPascalCase` (префікс `I`) | `IMovieRepository` |
| **Методи (Async)** | `PascalCase` + `Async` | `GetByIdAsync()`, `SaveAsync()` |
| **Приватні поля** | `_camelCase` (з підкресленням) | `private readonly DbContext _context;` |
| **Локальні змінні / Параметри** | `camelCase` | `var movieId = 10;` |
| **Константи** | `UPPER_SNAKE_CASE` | `MAX_RETRY_COUNT` |

🌿 2. Робота з Git та Workflow

Правила іменування гілок

Формат: тип/номер-завдання-короткий-опис

feature/SCRUM-12-health-check — нова функціональність.

bugfix/SCRUM-45-fix-db-conn — виправлення багів.

Правила комітів (Conventional Commits)

feat: (нова фіча), fix: (баг), docs: (документація), chore: (налаштування).

Приклад: feat: implement global exception handling middleware

✅ 3. Критерії готовності (Definition of Done - DoD)

Задача вважається завершеною, якщо:

Code Consistency: Код відповідає .editorconfig та правилам іменування.

Code Review: Отримано мінімум 1 схвалення (Approve) у Pull Request.

Testing: Написані та пройдені Unit-тести для бізнес-логіки.

Swagger: Методи API описані через XML-коментарі.

No Hardcode: Усі секрети винесено в .env або appsettings.json.

📁 4. Архітектурні шари (Onion Architecture)

Domain: Сутності та базові правила (без зовнішніх залежностей).

Application: Сценарії використання, сервіси, DTO, валідація.

Infrastructure: Реалізація БД (EF Core), зовнішні API, логування.

Presentation (API): Контролери, Middleware та налаштування Swagger.

🛠 5. Інструменти контролю якості

EditorConfig: Автоматичне форматування в IDE.

StyleCop.Analyzers: Контроль стилю коду (наприклад, перевірка відступів SA1137).

SonarAnalyzer: Пошук логічних помилок та вразливостей.