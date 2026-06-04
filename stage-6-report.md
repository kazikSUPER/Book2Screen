# Stage 6 — DB Engineer Report

**Дата виконання:** 05.06.2026
**Виконавець ролі:** DB Engineer
**Гілка:** `stage-6-dbopt...` (Book2Screen/Book2Screen.Test)

---

## Загальний огляд

На цьому етапі виконано повний цикл задач DB Engineer для підготовки Release Candidate:
валідація схеми, актуалізація документації, додавання performance-індексів та впровадження
механізму Schema Freezing для захисту цілісності RC.

---

## 1. Final Schema Validation + ERD Export

### Що зроблено

Проведено повний аудит відповідності між:
- усіма міграціями EF Core (`InitialCreate` → `AddUserAvatar`)
- `ApplicationDbContextModelSnapshot.cs`
- існуючим `DATA_DICTIONARY.md`

Виявлено **6 розбіжностей** між документацією та реальною схемою БД.

### Знайдені розбіжності

| # | Проблема | Таблиця/Поле |
|---|----------|--------------|
| 1 | Naming Convention описував `snake_case`, реально EF Core використовує `PascalCase` | Весь документ |
| 2 | Поле `Country` присутнє в snapshot та індексі, але не описане | `Adaptations.Country` |
| 3 | Поле `AvatarUrl` присутнє в snapshot, але не описане | `Users.AvatarUrl` |
| 4 | Три таблиці повністю відсутні в документації | `Favorites`, `PasswordResetTokens`, `Reports` |
| 5 | Два поля додані міграцією, але не відображені | `Reviews.Rating`, `Reviews.IsSpoiler` |
| 6 | Складений PK не відображено | `AdaptationActor (AdaptationId, ActorId, RoleName)` |

### Змінені файли

| Файл | Дія |
|------|-----|
| `DATA_DICTIONARY.md` | Повна актуалізація — оновлено/додано розділи 4.1–4.17, додано розділи 5 (індекси), 6 (CHECK-обмеження), 7 (журнал міграцій) |

### Що додано в DATA_DICTIONARY.md

- Опис усіх **17 таблиць** (було 10) з актуальними типами даних із snapshot
- Розділ 5 — **зведена таблиця всіх індексів** (13 індексів)
- Розділ 6 — **зведена таблиця всіх CHECK-обмежень** (5 обмежень)
- Розділ 7 — **журнал змін схеми** по всіх 8 міграціях

---

## 2. Performance Indexing

### Що зроблено

Проаналізовано всі 10 контролерів та визначено найнавантаженіші патерни запитів.
Створено нову міграцію EF Core з **8 composite індексами**.

### Аналіз запитів → індекси

| Ендпоінт | Патерн запиту | Доданий індекс |
|----------|--------------|----------------|
| `GET /api/v1/reviews/work/{workId}` | `WHERE WorkId = ? ORDER BY CreatedAt DESC` | `IX_Reviews_WorkId_CreatedAt` |
| `GET /api/v1/users/me/reviews` | `WHERE UserId = ? ORDER BY CreatedAt DESC` | `IX_Reviews_UserId_CreatedAt` |
| `GET /api/v1/admin/reports` | `WHERE Status = 'Pending' ORDER BY CreatedAt ASC` | `IX_Reports_Status_CreatedAt` |
| `GET /api/v1/votes/{workId}` | `WHERE WorkId = ? GROUP BY SelectedOption` | `IX_Votes_WorkId_SelectedOption` |
| `GET /api/v1/favorites` | `WHERE UserId = ? ORDER BY CreatedAt DESC` | `IX_Favorites_UserId_CreatedAt` |
| `POST /api/v1/auth/verify-code` + `reset-password` | `WHERE Email = ? AND IsUsed = false AND ExpiryTime > now()` | `IX_PasswordResetTokens_Email_IsUsed` |
| `GET /api/v1/works/top` | `ORDER BY AdaptationRating DESC LIMIT N` | `IX_Ratings_AdaptationRating` |
| Карта відмінностей (DifferenceMap) | `WHERE WorkId = ? AND SourceType = ? ORDER BY SequenceNumber` | `IX_PlotEvent_WorkId_SourceType_SequenceNumber` |

### Створені файли

| Файл | Розташування | Дія |
|------|-------------|-----|
| `20260605000001_AddPerformanceIndexes.cs` | `server/Migrations/` | Нова міграція: `Up()` — створює 8 індексів, `Down()` — видаляє |
| `20260605000001_AddPerformanceIndexes_Designer.cs` | `server/Migrations/` | Designer-файл міграції з атрибутом `[Migration(...)]` |

### Як застосувати

Міграція застосовується **автоматично** при наступному запуску контейнера у Development-середовищі через наявний `db.Database.MigrateAsync()` в `Program.cs`.

---

## 3. Schema Freezing

### Що зроблено

Впроваджено механізм захисту цілісності Release Candidate — перевірка pending міграцій
при старті backend-контейнера. Блокує запуск сервера якщо в білді є незастосовані міграції.

### Логіка роботи

```
ASPNETCORE_ENVIRONMENT = Development | Test
    → MigrateAsync() — застосовує міграції автоматично (поведінка без змін)

ASPNETCORE_ENVIRONMENT = Staging | Production | ReleaseCandidate
    → GetPendingMigrationsAsync()
        → 0 pending  : "Schema freeze check passed" → сервер стартує
        → N pending  : InvalidOperationException → сервер НЕ стартує
                       лог: SCHEMA FREEZE VIOLATION + перелік міграцій
```

### Захист від retry-loop

Додано окремий `catch (InvalidOperationException)` — freeze violation не потрапляє
в retry-цикл (не чекає 5с × 10 спроб), а зупиняє сервер негайно.

### Змінені файли

| Файл | Розташування | Зміни |
|------|-------------|-------|
| `Program.cs` | `server/` | Додано блок Schema Freezing (~30 рядків) у секцію ініціалізації БД |

### Як активувати для RC

У `compose.yaml` або середовищі деплою встановити:

```yaml
- ASPNETCORE_ENVIRONMENT=ReleaseCandidate
```

---

## 4. Зведена таблиця змінених файлів

| Файл | Розташування | Тип змін |
|------|-------------|----------|
| `DATA_DICTIONARY.md` | `docs/` або корінь репо | Оновлено (повна актуалізація) |
| `20260605000001_AddPerformanceIndexes.cs` | `server/Migrations/` | Створено (нова міграція) |
| `20260605000001_AddPerformanceIndexes_Designer.cs` | `server/Migrations/` | Створено (designer міграції) |
| `Program.cs` | `server/` | Оновлено (Schema Freezing) |

---

## 5. Стан схеми БД після етапу

### Таблиці: 17
### Індекси після етапу: 21 (13 існуючих + 8 нових)
### CHECK-обмеження: 5
### Остання міграція в RC: `20260605000001_AddPerformanceIndexes`

### Ланцюг міграцій (повний)

```
20260421114950_InitialCreate
20260424153950_AddVotesAndReviewUpdates
20260424155322_UpdateModelsFix
20260512073329_AddFavoritesAndPasswordReset
20260513114527_AddSearchIndexesAndFilter
20260513114558_UpdateSearchIndexesAndFilter
20260517120423_AddReports
20260517121337_AddUserAvatar
20260605000001_AddPerformanceIndexes   ← новий (Stage 6)
```





---

## Data Consistency Fixes

**Файл:** `docs/fix_data_consistency.sql`

Написано SQL-скрипт для виправлення аномалій даних. Скрипт виконується в транзакції (`BEGIN` / `COMMIT`) з можливістю `ROLLBACK`.

| # | FIX | Таблиця | Причина бага |
|---|-----|---------|-------------|
| 1 | `IsSpoiler = false` але текст містить `"Spoiler:"` | `Reviews` | `DbSeeder` створює відгук з коментарем `// Initial state, will be reported` — модерація могла не відпрацювати |
| 2 | `Status = 'Pending'` на видалені відгуки (`ReviewId = NULL`) | `Reports` | `ON DELETE SET NULL` скидає FK але статус не оновлюється |
| 3 | `SelectedOption = 'movie'` — застарілий варіант | `Votes` | CHECK дозволяє `'movie'`, але API використовує лише `'book'`/`'adaptation'` |
| 4 | `VotesCount = 0` але рейтинг виставлений | `Ratings` | Повторний seed після скидання даних |
| 5 | Дублікати (один твір двічі в обраному) | `Favorites` | Могли виникнути до появи UNIQUE індексу в міграції `AddFavoritesAndPasswordReset` |
| 6 | Протерміновані невикористані токени | `PasswordResetTokens` | Немає TTL / cleanup job |
| 7 | Користувачі без активності старші 7 днів | `Users` | Незавершена реєстрація або тестові акаунти |

Як запустити:
```bash
docker exec -i project_db psql -U $DB_USER -d $DB_NAME < fix_data_consistency.sql
```

---

## Schema Freezing

**Файл:** `server/Program.cs`

Додано блок перевірки цілісності схеми при старті контейнера (~30 рядків у секції ініціалізації БД).

Логіка:
- `Development` / `Test` — поведінка без змін, `MigrateAsync()` застосовує міграції автоматично
- `Staging` / `Production` / `ReleaseCandidate` — викликається `GetPendingMigrationsAsync()`: якщо є pending міграції — сервер **не стартує**, логується `SCHEMA FREEZE VIOLATION` з переліком порушників

Як активувати для RC — у `compose.yaml`:
```yaml
- ASPNETCORE_ENVIRONMENT=ReleaseCandidate
```







*Документ сформовано: 05.06.2026*
