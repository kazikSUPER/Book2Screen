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



Етап: DB Engineer — Dump & Scripts / Config Auditor
Seed-дані (DbSeeder.cs) — ✅ Виконано
Клас DbSeeder реалізовано з урахуванням принципу ідемпотентності: обидва блоки наповнення захищені перевірками через AnyAsync, що гарантує безпечний повторний запуск на вже заповненій базі даних без дублювання записів.
У результаті успішного виконання сідера до бази додаються наступні сутності:

User × 2 (адміністратор та звичайний користувач)
Author, Actor, Book, Adaptation, AdaptationActor
Work, Rating, Review, Report

Загалом — 10 пов'язаних записів, які формують повноцінний тестовий сценарій для перевірки основного функціоналу застосунку.
Окремо варто зазначити, що відгук користувача john_doe містить текст зі спойлером ("Paul survives!"), однак поле IsSpoiler виставлено у false. При цьому адміністратор вже створив скаргу (Report) на цей відгук зі статусом Pending. Це навмисний тестовий кейс, який дозволяє перевірити коректну роботу адміністративного інтерфейсу в частині обробки скарг.



## Етап: DB Engineer — Backup Strategy / Config Update

### Backup Strategy — Фінальний дамп бази даних (Snapshot)

Для забезпечення стабільної демонстрації проєкту було підготовлено фінальний SQL-snapshot бази даних — файл `book2screen_demo_snapshot_v1.sql`, розміщений у папці `docs/`.

Файл містить три логічні блоки:

**Структура бази даних** — повне визначення всіх 16 таблиць у правильному порядку залежностей (від незалежних сутностей до залежних), включаючи всі зовнішні ключі, індекси та CHECK-обмеження. Структура повністю відповідає фінальному стану міграцій EF Core.

**Історія міграцій** — таблиця `__EFMigrationsHistory` заповнена всіма 8 застосованими міграціями, що запобігає повторному їх виконанню при запуску застосунку після відновлення з дампу.

**Seed-дані** — всі демонстраційні записи з `DbSeeder.cs` вставлені з фіксованими UUID, що гарантує коректність зв'язків між таблицями після кожного відновлення. Включено: 2 користувачі (admin, john\_doe), автор, актор, книга, екранізація, work, рейтинг, відгук та скарга.

Файл є ідемпотентним: блок `DROP TABLE IF EXISTS ... CASCADE` на початку та директива `ON CONFLICT DO NOTHING` для seed-даних дозволяють безпечно запускати snapshot повторно без помилок.

Команда для відновлення перед демонстрацією:

```bash
# Через Docker (відповідно до compose.yaml):
docker exec -i project_db psql -U postgres -d book2screen \
  < docs/book2screen_demo_snapshot_v1.sql
```

---

### Оновлення файлу .env.example

У файлі `.env.example` було усунено всі заглушки та уточнено конфігурацію відповідно до реальних параметрів проєкту. Зміни наведено в таблиці нижче:

| Параметр | До | Після | Причина |
|---|---|---|---|
| `DB_CONNECTION_STRING` | `your_database_connection_string_here` | Реальний приклад рядка підключення PostgreSQL | Розробники одразу бачать формат |
| `JWT_SECRET` | `generate_long_random_string_here` | Опис вимоги (мін. 32 символи) | Усувається неоднозначність |
| `JWT_ISSUER` | `your_app_name` | `Book2Screen` | Відповідає реальному імені застосунку |
| `JWT_AUDIENCE` | `your_app_users` | `Book2ScreenUsers` | Відповідає реальній аудиторії токена |
| `SENDER_EMAIL` | `your-email@gmail.com` | `noreply@book2screen.com` | Відображає реальну адресу відправника |
| `SENDER_PASSWORD` | `your-app-password` | Опис формату Gmail App Password + посилання на інструкцію | Усувається плутанина з паролем акаунта |
| `API_URL_EXTERNAL` | Без коментаря | Додано примітку про перехід на `https://` у staging/prod | Попередження про безпеку |


















фінальна ERD
https://mermaid.live/edit#pako:eNrdWG1v4jgQ_iuRpf1GqwItb99oae-qbXsVlD3dCmnlJlOwSOKcY9Oy0P--tkNI4pgSLVw_HB-qJvPMeF4ej8dZIZd6gHoI2IDgKcPBJHTkbxwDi51V8qB-QhDPufWcx6_ZuwVm7gwzDQ5xAM7YIrwOMPELEg5v3HnEcfxKmfcnjmdlpSH1ofy2v8AcszHzM9Ezpb5zG_ddThY5DU4CiDkOIueKAebg9blNOI68vPB9Eib_9AWf0erx3wjff5DxGyFeEioTGs2WtqUvCeOzgVzdUHrAnNAQ-4QvjxbOJaXzysE8Ee6bTg0gdhmJlGeG5A8ImYm-w-FU4GnuNQllwcWzT1wd3T-AmaFzRRfAbgOpVSjvgYFLdsx_4B3F3BRZp6F289WQ6pwVZRk9XP4_YkffwxHXdn-XI9vXy6gScxQdhuADjqFIBSUYCKaduSeh4BAb9kZceISWyCNCzpZmh6ExB3ZMOmWZ0gwoM2or30EqrVaW5bueYkoh55s6HC2Ivyk7rBmMRBDgfLa3-0UaGduizqfFABwYzKNP-fUCJG2O3d0UFUfwr4DQhQcRPAMrGxtRwVwosl4vrnIsHThioAPy8gJMOXOPo98tn4rpmzzXC4HmHT5ucTKf9zq8uw5pKJmxYr5T-W0QUcaxlN_BAnxjOZk0ox5b0mr62IQZbW2QA3MzhAWB12p5eZJ_slcelQcpOEPpWTgtDUKjiBIfjJZ6R-YQ6yZpoQhmU-AWDquhzpaW43N7CKp0lUk9lOeGjSEjWSwRGzaSPNvisMR3YBzfKIfKUYzk-edKK38ZfP_k3N_gBWWkgt-fzQlN771ehSIARly9kc0dkcqyfWwidD9URTP2xn_XEtNbzxBiuenoHMJqLUBfosokupJ3N9vy128RYcsnkh86Nw1ClvB4s8SXL2qOS2bHGYni4u1pvT45oaviEN5zJmg7eE9Q_n6yC76ZxBXYhK_Xm3EmxWU281NtCZqfSDK7pop0xpz4bMq5G8FHWsnol62WeLPRyOYYhU2ol9rOAdfmHLAHne6iD2Gr7WG0B5b0tz2grKEUgdYxZqOTHxSUlj6tU-tZanagc4d4ZZ3S2Z65mHz5sGQmaX7pEgXYrswUQMXMfGAtPQ-LsOLcYIGmR90EoRqaMuKhHmcCaki2Qdk-5CPSrWaC-AxkZ0BKycNsrqy_S50Ih98pDVI1RsV0hnov2I_lk9AtYPONaAuB0AOmuyfq1RvaBOqt0BvqNc46p-2Lbr3danU6zfN6s4aWqNfsntYb5912t1lvN1utxsV7Df3Ua56ddtoXZ_JXP290Wo16t4bAI3LP3Cffp_Rnqvdfx4y9vg




*Документ сформовано: 05.06.2026*
