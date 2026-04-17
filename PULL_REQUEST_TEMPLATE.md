📝 Опис (Summary)

Додано реалізацію Health Check для PostgreSQL.

Налаштовано Global Exception Handling для повернення стандартизованого JSON.

🔗 Завдання (Task)

ID: #SCRUM-123

Link: [підозріле посилання видалено]

🛠 Тип змін (Type of Change)

[ ] feat: Новий функціонал

[ ] fix: Виправлення помилки

[ ] docs: Зміни в документації

[ ] refactor: Покращення структури коду (без зміни логіки)

[ ] chore: Оновлення конфігурацій, бібліотек тощо

🏗 Технічні деталі (Technical Details)

Використано інтерфейс IExceptionHandler (.NET 8).

Додано пакет AspNetCore.HealthChecks.Npgsql для моніторингу БД.

Дані для підключення винесено в appsettings.json та зчитано через IConfiguration.

✅ Чек-лист (Definition of Done)

[ ] Code Style: Код відповідає .editorconfig та QUALITY_STANDARDS.md.

[ ] Tests: Написані Unit-тести для нових сервісів, статус — Passed.

[ ] Documentation: Додано XML-коментарі для Swagger.

[ ] Migrations: (Якщо потрібно) Створено нову міграцію та перевірено її застосування.

[ ] No Hardcode: Відсутні секрети (паролі/токени) безпосередньо в коді.

[ ] Self-Review: Я перевірив свій код на наявність зайвих Console.WriteLine чи закоментованих блоків.

📸 Скріншоти / Логи (Optional)

(Додайте скріншот успішного запиту до /health або результат Swagger тут)