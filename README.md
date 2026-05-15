# DB Engineer Report — Stage 4

## Project

Book2Screen / BookScreenExplorer

## Branch

`DB-structure-optimization`

---

# Етап 4 — Active Development

## Виконані завдання DB Engineer

У межах Етапу 4 було виконано оптимізацію структури бази даних, реалізовано нові сутності, створено складні SQL-запити, перевірено constraints та оновлено документацію.

---

# 1. DB Optimization

Було виконано перевірку та оптимізацію SQL-запитів.

## Додані індекси

| Таблиця     | Поле               |
| ----------- | ------------------ |
| works       | title              |
| books       | genre              |
| adaptations | country            |
| favorites   | (user_id, work_id) |

## EXPLAIN Queries

Було створено файл:

```plaintext
BookScreenExplorer.Infrastructure/Database/explain_queries.sql
```

У файлі реалізовано `EXPLAIN` для:

* пошуку;
* фільтрації;
* JOIN-запитів;
* GROUP BY;
* звітів;
* favorites lookup.

Результат перевірки підтвердив коректне використання індексів для оптимізації запитів.

---

# 2. Complex Queries

Було створено файл:

```plaintext
BookScreenExplorer.Infrastructure/Database/complex_queries.sql
```

Реалізовано складні SQL-запити для:

* JOIN;
* GROUP BY;
* COUNT;
* AVG;
* фільтрації;
* пошуку;
* звітів.

## Реалізовані сценарії

* звіт по рейтингах;
* статистика жанрів;
* пошук творів;
* звіт по favorites;
* звіт по differences;
* активність користувачів.

---

# 3. Incremental Migrations

Було створено нову міграцію:

```plaintext
20260515000100_AddFavoritesPasswordResetAndDbOptimizations.cs
```

Міграція включає:

* таблицю `favorites`;
* таблицю `password_reset_tokens`;
* індекси;
* foreign keys;
* constraints.

---

# 4. Data Integrity

Було створено файл:

```plaintext
BookScreenExplorer.Infrastructure/Database/data_integrity_tests.sql
```

У файлі реалізовано перевірку:

* CHECK constraints;
* UNIQUE constraints;
* Foreign Keys;
* integrity rules.

## Перевірені сценарії

* неправильний рейтинг;
* неправильне голосування;
* дублювання favorites;
* invalid foreign keys;
* неправильний difference type;
* неправильний importance level.

Результат: constraints та foreign keys коректно блокують невалідні дані.

---

# 5. Backup / Seed Update

Було оновлено `SeedDataExtensions.cs`.

Додано seed-дані для:

* Favorites;
* DifferenceMaps;
* Differences;
* PasswordResetTokens.

Це дозволило QA Engineer тестувати:

* relationships;
* search/filter logic;
* favorites;
* difference system.

---

# 6. Оновлення структури БД

Було додано нові сутності:

* PasswordResetTokens;
* Favorites.

Також було оновлено:

* DifferenceMaps;
* Differences;
* ApplicationDbContext;
* Fluent API configuration.

Було реалізовано:

* relationships;
* constraints;
* delete behaviors;
* unique indexes.

---

# 7. Оновлена ER-діаграма

Було створено файл:

```plaintext
docs/ERD.md
```

ER-діаграма містить:

* усі таблиці системи;
* primary keys;
* foreign keys;
* relationships;
* нові сутності;
* One-To-One;
* One-To-Many;
* Many-To-Many зв’язки.

---

# 8. Підсумковий результат

У результаті виконання Етапу 4 було:

* оптимізовано SQL-запити;
* створено індекси;
* реалізовано EXPLAIN-перевірки;
* додано складні SQL-запити;
* перевірено Data Integrity;
* оновлено seed-дані;
* створено нові міграції;
* оновлено ER-діаграму.

База даних підготовлена для стабільної інтеграції з Backend та Frontend частиною проєкту.
