# DATA_DICTIONARY.md

# Словник даних (Data Dictionary)

## 1. Призначення документа

Документ **DATA_DICTIONARY.md** є технічним описом фізичної моделі даних для платформи **Book ↔ Screen Explorer**, побудованої для **PostgreSQL**. У цьому словнику даних зафіксовано всі сутності, що входять до ER-діаграми, а також атрибути кожної сутності, типи даних, первинні ключі, зовнішні ключі, обмеження цілісності та додаткові правила зберігання даних.

Основна мета документа:

* надати команді розробки єдине джерело опису структури бази даних;
* забезпечити узгодженість між ER-діаграмою, міграціями EF Core та реалізацією в PostgreSQL;
* зафіксувати всі таблиці, поля та їхні характеристики;
* пояснити логіку використання кожної сутності;
* підтвердити відповідність фізичної моделі правилам іменування з **Code Style Guide**.

Цей документ сформований на основі `ApplicationDbContextModelSnapshot.cs` та всіх міграцій EF Core станом на **17.05.2026** (остання міграція: `AddUserAvatar`).

---

## 2. Конвенції іменування (Naming Convention)

У EF Core моделі використовується **PascalCase** для назв таблиць та полів (стандарт C# / EF Core).

### Використані правила:

* назви таблиць записуються у множині з великої літери: `Users`, `Books`, `Reviews`, `Votes`;
* назви полів у PascalCase: `CreatedAt`, `UpdatedAt`, `PublicationYear`;
* первинний ключ у всіх таблицях має назву `Id` (тип `uuid`);
* зовнішні ключі мають формат `<Entity>Id`: `UserId`, `BookId`, `WorkId`, `MapId`;
* виняток: таблиця `book_authors` — назва задана явно через `.ToTable("book_authors")`.

---

## 3. Загальна характеристика фізичної моделі

Фізична модель бази даних охоплює **16 таблиць**. Центральною сутністю є `Works`, яка поєднує одну книгу (`Books`) та одну екранізацію (`Adaptations`) в межах одного об'єкта порівняння. Навколо цієї сутності організовані таблиці користувацької взаємодії (`Reviews`, `Votes`, `Ratings`, `Favorites`), таблиці предметної області (`Books`, `Authors`, `Adaptations`, `Actors`), допоміжні таблиці безпеки (`PasswordResetTokens`), таблиця звітності (`Reports`), а також таблиці для побудови карти відмінностей (`PlotEvent`, `DifferenceMaps`, `Differences`).

### Повний перелік таблиць:

| № | Таблиця               | Призначення                                      |
|---|-----------------------|--------------------------------------------------|
| 1 | `Users`               | Облікові записи користувачів                     |
| 2 | `Authors`             | Автори книг                                      |
| 3 | `Books`               | Книги                                            |
| 4 | `Adaptations`         | Екранізації (фільми / серіали)                   |
| 5 | `Actors`              | Актори                                           |
| 6 | `Works`               | Центральна сутність: пара книга + екранізація    |
| 7 | `book_authors`        | M2M: книги ↔ автори                              |
| 8 | `AdaptationActor`     | M2M: екранізації ↔ актори (з роллю)              |
| 9 | `Reviews`             | Відгуки користувачів                             |
| 10| `Votes`               | Голосування (книга / екранізація)                |
| 11| `Ratings`             | Агрегований рейтинг до `Works`                   |
| 12| `Favorites`           | Закладки/обране користувача                      |
| 13| `PasswordResetTokens` | Токени скидання пароля                           |
| 14| `Reports`             | Скарги на відгуки                                |
| 15| `PlotEvent`           | Події сюжету (книга або екранізація)             |
| 16| `DifferenceMaps`      | Карта відмінностей між книгою та екранізацією    |
| 17| `Differences`         | Окремі відмінності в межах карти                 |

### Стратегії видалення:

* `ON DELETE CASCADE` — якщо дочірній запис не має сенсу без батьківського;
* `ON DELETE SET NULL` — якщо дочірній запис повинен зберігатися як історичний (відгуки, звіти після видалення користувача).

---

## 4. Опис сутностей та їх атрибутів

### 4.1. Таблиця `Users`

**Призначення:** Облікові записи користувачів платформи. Містить дані для автентифікації, авторизації та відображення профілю.

| Поле           | Тип (PostgreSQL)         | Обмеження                        | Опис                                           |
|----------------|--------------------------|----------------------------------|------------------------------------------------|
| `Id`           | `uuid`                   | PK, NOT NULL                     | Унікальний ідентифікатор                       |
| `Username`     | `character varying(50)`  | NOT NULL, UNIQUE                 | Унікальне ім'я користувача                     |
| `Email`        | `character varying(255)` | NOT NULL, UNIQUE                 | Електронна адреса                              |
| `PasswordHash` | `text`                   | NOT NULL                         | Хеш пароля                                     |
| `Role`         | `character varying(20)`  | NOT NULL, CHECK                  | Роль: `user`, `admin`, `moderator`             |
| `IsActive`     | `boolean`                | NOT NULL                         | Прапорець активності акаунта                   |
| `AvatarUrl`    | `character varying(1000)`| NULL                             | URL аватара користувача *(додано: 17.05.2026)* |
| `CreatedAt`    | `timestamp with time zone`| NOT NULL                        | Дата створення запису                          |
| `UpdatedAt`    | `timestamp with time zone`| NOT NULL                        | Дата останнього оновлення                      |

**CHECK:** `CK_User_Role` — `Role IN ('user', 'admin', 'moderator')`

**Індекси:** `IX_Users_Email` (UNIQUE), `IX_Users_Username` (UNIQUE)

**Зв'язки (дочірні таблиці):** `Reviews` (SET NULL), `Votes` (CASCADE), `Favorites` (CASCADE), `Reports` (SET NULL)

---

### 4.2. Таблиця `Authors`

**Призначення:** Довідник авторів книг.

| Поле          | Тип (PostgreSQL)         | Обмеження    | Опис                        |
|---------------|--------------------------|--------------|-----------------------------|
| `Id`          | `uuid`                   | PK, NOT NULL | Унікальний ідентифікатор    |
| `FullName`    | `character varying(150)` | NOT NULL     | Повне ім'я автора           |
| `BirthDate`   | `timestamp with time zone`| NULL        | Дата народження             |
| `Nationality` | `text`                   | NULL         | Національність              |
| `Biography`   | `text`                   | NULL         | Біографічна довідка         |
| `CreatedAt`   | `timestamp with time zone`| NOT NULL    | Дата створення запису       |
| `UpdatedAt`   | `timestamp with time zone`| NOT NULL    | Дата останнього оновлення   |

**Зв'язки:** M2M з `Books` через `book_authors`

---

### 4.3. Таблиця `Books`

**Призначення:** Книги, що є основою для порівняння з екранізаціями.

| Поле              | Тип (PostgreSQL)         | Обмеження    | Опис                        |
|-------------------|--------------------------|--------------|-----------------------------|
| `Id`              | `uuid`                   | PK, NOT NULL | Унікальний ідентифікатор    |
| `Title`           | `character varying(255)` | NOT NULL     | Назва книги                 |
| `Description`     | `text`                   | NULL         | Опис / анотація             |
| `Genre`           | `text`                   | NULL         | Жанр                        |
| `PublicationYear` | `integer`                | NULL         | Рік публікації              |
| `Language`        | `text`                   | NULL         | Мова оригіналу              |
| `CoverImageUrl`   | `text`                   | NULL         | URL обкладинки              |
| `CreatedAt`       | `timestamp with time zone`| NOT NULL    | Дата створення              |
| `UpdatedAt`       | `timestamp with time zone`| NOT NULL    | Дата оновлення              |

**Індекси:** `IX_Books_Genre`

**Зв'язки:** 1:1 з `Works` (CASCADE); M2M з `Authors` через `book_authors`

---

### 4.4. Таблиця `Adaptations`

**Призначення:** Екранізації книг у форматі фільму або серіалу.

| Поле              | Тип (PostgreSQL)         | Обмеження    | Опис                                    |
|-------------------|--------------------------|--------------|-----------------------------------------|
| `Id`              | `uuid`                   | PK, NOT NULL | Унікальний ідентифікатор                |
| `Title`           | `character varying(255)` | NOT NULL     | Назва екранізації                       |
| `Type`            | `character varying(20)`  | NOT NULL     | Тип: `movie` або `series`               |
| `Description`     | `text`                   | NULL         | Опис                                    |
| `ReleaseYear`     | `integer`                | NULL         | Рік виходу                              |
| `DurationMinutes` | `integer`                | NULL         | Тривалість у хвилинах                   |
| `PosterUrl`       | `text`                   | NULL         | URL постера                             |
| `Studio`          | `text`                   | NULL         | Студія-виробник                         |
| `Country`         | `text`                   | NULL         | Країна виробництва *(додано пізніше)*   |
| `CreatedAt`       | `timestamp with time zone`| NOT NULL    | Дата створення                          |
| `UpdatedAt`       | `timestamp with time zone`| NOT NULL    | Дата оновлення                          |

**Індекси:** `IX_Adaptations_Country`

**Зв'язки:** 1:1 з `Works` (CASCADE); M2M з `Actors` через `AdaptationActor`

---

### 4.5. Таблиця `Actors`

**Призначення:** Довідник акторів, що беруть участь в екранізаціях.

| Поле          | Тип (PostgreSQL)         | Обмеження    | Опис                      |
|---------------|--------------------------|--------------|---------------------------|
| `Id`          | `uuid`                   | PK, NOT NULL | Унікальний ідентифікатор  |
| `FullName`    | `character varying(150)` | NOT NULL     | Повне ім'я актора         |
| `BirthDate`   | `timestamp with time zone`| NULL        | Дата народження           |
| `Nationality` | `text`                   | NULL         | Національність            |
| `Biography`   | `text`                   | NULL         | Коротка біографія         |
| `CreatedAt`   | `timestamp with time zone`| NOT NULL    | Дата створення            |
| `UpdatedAt`   | `timestamp with time zone`| NOT NULL    | Дата оновлення            |

**Зв'язки:** M2M з `Adaptations` через `AdaptationActor`

---

### 4.6. Таблиця `Works`

**Призначення:** Центральна сутність моделі. Об'єднує книгу та екранізацію в один об'єкт порівняння.

| Поле           | Тип (PostgreSQL)         | Обмеження          | Опис                               |
|----------------|--------------------------|--------------------|------------------------------------|
| `Id`           | `uuid`                   | PK, NOT NULL       | Унікальний ідентифікатор           |
| `BookId`       | `uuid`                   | FK, NOT NULL, UNIQUE| Посилання на `Books`              |
| `AdaptationId` | `uuid`                   | FK, NOT NULL, UNIQUE| Посилання на `Adaptations`        |
| `Title`        | `character varying(255)` | NOT NULL           | Назва сторінки порівняння          |
| `Summary`      | `text`                   | NULL               | Узагальнений опис                  |
| `CreatedAt`    | `timestamp with time zone`| NOT NULL          | Дата створення                     |
| `UpdatedAt`    | `timestamp with time zone`| NOT NULL          | Дата оновлення                     |

**Індекси:** `IX_Works_BookId` (UNIQUE), `IX_Works_AdaptationId` (UNIQUE), `IX_Works_Title`

**Зовнішні ключі:** `BookId` → `Books.Id` (CASCADE), `AdaptationId` → `Adaptations.Id` (CASCADE)

**Дочірні таблиці:** `Reviews`, `Votes`, `Ratings`, `Favorites`, `PlotEvent`, `DifferenceMaps`

---

### 4.7. Таблиця `book_authors`

**Призначення:** Реалізує зв'язок M2M між `Books` та `Authors`.

| Поле        | Тип (PostgreSQL) | Обмеження    | Опис                          |
|-------------|------------------|--------------|-------------------------------|
| `AuthorsId` | `uuid`           | PK (part), FK| Посилання на `Authors`        |
| `BooksId`   | `uuid`           | PK (part), FK| Посилання на `Books`          |

> **Примітка:** Складний PK `(AuthorsId, BooksId)`. Таблиця не має окремого поля `Id`.

**Зовнішні ключі:** `AuthorsId` → `Authors.Id` (CASCADE), `BooksId` → `Books.Id` (CASCADE)

---

### 4.8. Таблиця `AdaptationActor`

**Призначення:** Реалізує зв'язок M2M між `Adaptations` та `Actors` з додатковим атрибутом — роллю актора.

| Поле           | Тип (PostgreSQL)         | Обмеження    | Опис                                    |
|----------------|--------------------------|--------------|-----------------------------------------|
| `AdaptationId` | `uuid`                   | PK (part), FK| Посилання на `Adaptations`              |
| `ActorId`      | `uuid`                   | PK (part), FK| Посилання на `Actors`                   |
| `RoleName`     | `character varying(150)` | PK (part)    | Назва ролі (входить до складеного PK)   |
| `Id`           | `uuid`                   | NOT NULL     | Додатковий UUID (не PK)                 |
| `CreatedAt`    | `timestamp with time zone`| NOT NULL    | Дата створення                          |
| `UpdatedAt`    | `timestamp with time zone`| NOT NULL    | Дата оновлення                          |

> **Примітка:** Складний PK `(AdaptationId, ActorId, RoleName)` — дозволяє одному актору грати кілька ролей в одній екранізації.

**Зовнішні ключі:** `AdaptationId` → `Adaptations.Id` (CASCADE), `ActorId` → `Actors.Id` (CASCADE)

---

### 4.9. Таблиця `Reviews`

**Призначення:** Відгуки користувачів на книги або екранізації в межах конкретного `Work`.

| Поле         | Тип (PostgreSQL)        | Обмеження    | Опис                                          |
|--------------|-------------------------|--------------|-----------------------------------------------|
| `Id`         | `uuid`                  | PK, NOT NULL | Унікальний ідентифікатор                      |
| `WorkId`     | `uuid`                  | FK, NOT NULL | Посилання на `Works`                          |
| `UserId`     | `uuid`                  | FK, NULL     | Посилання на `Users` (NULL після видалення)   |
| `Text`       | `text`                  | NOT NULL     | Текст відгуку                                 |
| `TargetType` | `character varying(20)` | NOT NULL     | Об'єкт відгуку: книга або екранізація         |
| `Rating`     | `double precision`      | NOT NULL     | Оцінка відгуку *(додано: 24.04.2026)*         |
| `IsSpoiler`  | `boolean`               | NOT NULL     | Прапорець спойлера *(додано: 24.04.2026)*     |
| `LikesCount` | `integer`               | NOT NULL     | Кількість лайків                              |
| `CreatedAt`  | `timestamp with time zone`| NOT NULL   | Дата створення                                |
| `UpdatedAt`  | `timestamp with time zone`| NOT NULL   | Дата оновлення                                |

**Індекси:** `IX_Reviews_UserId`, `IX_Reviews_WorkId`

**Зовнішні ключі:** `WorkId` → `Works.Id` (CASCADE), `UserId` → `Users.Id` (SET NULL)

---

### 4.10. Таблиця `Votes`

**Призначення:** Голосування користувачів — за книгу або екранізацію.

| Поле             | Тип (PostgreSQL)        | Обмеження    | Опис                                          |
|------------------|-------------------------|--------------|-----------------------------------------------|
| `Id`             | `uuid`                  | PK, NOT NULL | Унікальний ідентифікатор                      |
| `WorkId`         | `uuid`                  | FK, NOT NULL | Посилання на `Works`                          |
| `UserId`         | `uuid`                  | FK, NULL     | Посилання на `Users`                          |
| `SelectedOption` | `character varying(20)` | NOT NULL     | Вибір: `book`, `adaptation`, `movie`          |
| `CreatedAt`      | `timestamp with time zone`| NOT NULL   | Дата створення                                |
| `UpdatedAt`      | `timestamp with time zone`| NOT NULL   | Дата оновлення                                |

**CHECK:** `CK_Vote_Option` — `SelectedOption IN ('book', 'adaptation', 'movie')`

**Індекси:** `IX_Votes_WorkId`, `IX_Votes_UserId_WorkId` (UNIQUE)

**Зовнішні ключі:** `WorkId` → `Works.Id` (CASCADE), `UserId` → `Users.Id` (CASCADE)

---

### 4.11. Таблиця `Ratings`

**Призначення:** Агрегований рейтинг для кожного `Work` (1:1). Зберігає підсумкові оцінки книги та екранізації.

| Поле               | Тип (PostgreSQL) | Обмеження        | Опис                           |
|--------------------|------------------|------------------|--------------------------------|
| `Id`               | `uuid`           | PK, NOT NULL     | Унікальний ідентифікатор       |
| `WorkId`           | `uuid`           | FK, NOT NULL, UNIQUE| Посилання на `Works`        |
| `BookRating`       | `numeric`        | NULL, CHECK ≥0 ≤10| Рейтинг книги                 |
| `AdaptationRating` | `numeric`        | NULL, CHECK ≥0 ≤10| Рейтинг екранізації           |
| `VotesCount`       | `integer`        | NOT NULL         | Кількість голосів              |
| `CreatedAt`        | `timestamp with time zone`| NOT NULL| Дата створення               |
| `UpdatedAt`        | `timestamp with time zone`| NOT NULL| Дата оновлення               |

**CHECK:** `CK_Rating_Book` — `BookRating BETWEEN 0 AND 10`; `CK_Rating_Adaptation` — `AdaptationRating BETWEEN 0 AND 10`

**Індекси:** `IX_Ratings_WorkId` (UNIQUE)

**Зовнішні ключі:** `WorkId` → `Works.Id` (CASCADE)

---

### 4.12. Таблиця `Favorites`

**Призначення:** Закладки — зберігає `Works`, додані користувачем до обраного. *(Додано: 12.05.2026)*

| Поле        | Тип (PostgreSQL) | Обмеження    | Опис                     |
|-------------|------------------|--------------|--------------------------|
| `Id`        | `uuid`           | PK, NOT NULL | Унікальний ідентифікатор |
| `UserId`    | `uuid`           | FK, NOT NULL | Посилання на `Users`     |
| `WorkId`    | `uuid`           | FK, NOT NULL | Посилання на `Works`     |
| `CreatedAt` | `timestamp with time zone`| NOT NULL| Дата додавання        |
| `UpdatedAt` | `timestamp with time zone`| NOT NULL| Дата оновлення        |

**Індекси:** `IX_Favorites_WorkId`, `IX_Favorites_UserId_WorkId` (UNIQUE — один запис на пару)

**Зовнішні ключі:** `UserId` → `Users.Id` (CASCADE), `WorkId` → `Works.Id` (CASCADE)

---

### 4.13. Таблиця `PasswordResetTokens`

**Призначення:** Токени (одноразові коди) для скидання паролю через email. *(Додано: 12.05.2026)*

| Поле         | Тип (PostgreSQL)        | Обмеження    | Опис                           |
|--------------|-------------------------|--------------|--------------------------------|
| `Id`         | `uuid`                  | PK, NOT NULL | Унікальний ідентифікатор       |
| `Email`      | `text`                  | NOT NULL     | Email, для якого видано токен  |
| `Code`       | `character varying(10)` | NOT NULL     | Одноразовий код                |
| `ExpiryTime` | `timestamp with time zone`| NOT NULL   | Час закінчення дії токена      |
| `IsUsed`     | `boolean`               | NOT NULL     | Чи використано токен           |
| `CreatedAt`  | `timestamp with time zone`| NOT NULL   | Дата створення                 |
| `UpdatedAt`  | `timestamp with time zone`| NOT NULL   | Дата оновлення                 |

> **Примітка:** Не має FK на `Users` — токен прив'язаний до email, а не до id користувача. Це дозволяє скидати пароль навіть до входу в систему.

---

### 4.14. Таблиця `Reports`

**Призначення:** Скарги користувачів на відгуки для модерації. *(Додано: 17.05.2026)*

| Поле       | Тип (PostgreSQL)        | Обмеження    | Опис                                            |
|------------|-------------------------|--------------|-------------------------------------------------|
| `Id`       | `uuid`                  | PK, NOT NULL | Унікальний ідентифікатор                        |
| `ReviewId` | `uuid`                  | FK, NULL     | Посилання на `Reviews` (NULL після видалення)   |
| `UserId`   | `uuid`                  | FK, NULL     | Посилання на `Users` (NULL після видалення)     |
| `Reason`   | `character varying(500)`| NOT NULL     | Причина скарги                                  |
| `Status`   | `character varying(20)` | NOT NULL     | Статус: `Pending`, `Resolved`, `Dismissed`      |
| `CreatedAt`| `timestamp with time zone`| NOT NULL   | Дата створення                                  |
| `UpdatedAt`| `timestamp with time zone`| NOT NULL   | Дата оновлення                                  |

**CHECK:** `CK_Report_Status` — `Status IN ('Pending', 'Resolved', 'Dismissed')`

**Індекси:** `IX_Reports_ReviewId`, `IX_Reports_UserId`

**Зовнішні ключі:** `ReviewId` → `Reviews.Id` (SET NULL), `UserId` → `Users.Id` (SET NULL)

---

### 4.15. Таблиця `PlotEvent`

**Призначення:** Події сюжету — елементи для побудови карти відмінностей між книгою та екранізацією.

| Поле             | Тип (PostgreSQL)        | Обмеження    | Опис                                           |
|------------------|-------------------------|--------------|------------------------------------------------|
| `Id`             | `uuid`                  | PK, NOT NULL | Унікальний ідентифікатор                       |
| `WorkId`         | `uuid`                  | FK, NOT NULL | Посилання на `Works`                           |
| `Title`          | `character varying(255)`| NOT NULL     | Назва події                                    |
| `Description`    | `text`                  | NULL         | Опис події                                     |
| `SourceType`     | `character varying(20)` | NOT NULL     | Джерело: книга або екранізація                 |
| `SequenceNumber` | `integer`               | NOT NULL     | Порядковий номер події в межах `Work`          |
| `CreatedAt`      | `timestamp with time zone`| NOT NULL   | Дата створення                                 |
| `UpdatedAt`      | `timestamp with time zone`| NOT NULL   | Дата оновлення                                 |

**Індекси:** `IX_PlotEvent_WorkId`

**Зовнішні ключі:** `WorkId` → `Works.Id` (CASCADE)

---

### 4.16. Таблиця `DifferenceMaps`

**Призначення:** Карта відмінностей між книгою та екранізацією. Один `Work` має рівно одну карту (1:1).

| Поле        | Тип (PostgreSQL)         | Обмеження         | Опис                           |
|-------------|--------------------------|-------------------|--------------------------------|
| `Id`        | `uuid`                   | PK, NOT NULL      | Унікальний ідентифікатор       |
| `WorkId`    | `uuid`                   | FK, NOT NULL, UNIQUE| Посилання на `Works`         |
| `Title`     | `character varying(255)` | NOT NULL          | Назва карти                    |
| `Version`   | `integer`                | NOT NULL          | Версія карти                   |
| `CreatedAt` | `timestamp with time zone`| NOT NULL         | Дата створення                 |
| `UpdatedAt` | `timestamp with time zone`| NOT NULL         | Дата оновлення                 |

**Індекси:** `IX_DifferenceMaps_WorkId` (UNIQUE)

**Зовнішні ключі:** `WorkId` → `Works.Id` (CASCADE)

---

### 4.17. Таблиця `Differences`

**Призначення:** Конкретні відмінності між сюжетними подіями книги та екранізації в межах карти.

| Поле                | Тип (PostgreSQL)        | Обмеження    | Опис                                              |
|---------------------|-------------------------|--------------|---------------------------------------------------|
| `Id`                | `uuid`                  | PK, NOT NULL | Унікальний ідентифікатор                          |
| `MapId`             | `uuid`                  | FK, NOT NULL | Посилання на `DifferenceMaps`                     |
| `BookEventId`       | `uuid`                  | FK, NULL     | Посилання на подію книги в `PlotEvent`            |
| `AdaptationEventId` | `uuid`                  | FK, NULL     | Посилання на подію екранізації в `PlotEvent`      |
| `Description`       | `text`                  | NOT NULL     | Опис відмінності                                  |
| `DifferenceType`    | `character varying(20)` | NOT NULL     | Тип відмінності                                   |
| `ImportanceLevel`   | `character varying(20)` | NOT NULL     | Рівень важливості                                 |
| `CreatedAt`         | `timestamp with time zone`| NOT NULL   | Дата створення                                    |
| `UpdatedAt`         | `timestamp with time zone`| NOT NULL   | Дата оновлення                                    |

**Індекси:** `IX_Differences_MapId`, `IX_Differences_BookEventId`, `IX_Differences_AdaptationEventId`

**Зовнішні ключі:** `MapId` → `DifferenceMaps.Id` (CASCADE), `BookEventId` → `PlotEvent.Id` (no action), `AdaptationEventId` → `PlotEvent.Id` (no action)

---

## 5. Зведена таблиця індексів

| Таблиця           | Індекс                          | Тип      | Додано міграцією                        |
|-------------------|---------------------------------|----------|-----------------------------------------|
| `Users`           | `IX_Users_Email`                | UNIQUE   | `InitialCreate`                         |
| `Users`           | `IX_Users_Username`             | UNIQUE   | `InitialCreate`                         |
| `Works`           | `IX_Works_AdaptationId`         | UNIQUE   | `InitialCreate`                         |
| `Works`           | `IX_Works_BookId`               | UNIQUE   | `InitialCreate`                         |
| `Works`           | `IX_Works_Title`                | Regular  | `UpdateSearchIndexesAndFilter`          |
| `Books`           | `IX_Books_Genre`                | Regular  | `UpdateSearchIndexesAndFilter`          |
| `Adaptations`     | `IX_Adaptations_Country`        | Regular  | `UpdateSearchIndexesAndFilter`          |
| `Votes`           | `IX_Votes_UserId_WorkId`        | UNIQUE   | `AddVotesAndReviewUpdates`              |
| `Favorites`       | `IX_Favorites_UserId_WorkId`    | UNIQUE   | `AddFavoritesAndPasswordReset`          |
| `Ratings`         | `IX_Ratings_WorkId`             | UNIQUE   | `InitialCreate`                         |
| `DifferenceMaps`  | `IX_DifferenceMaps_WorkId`      | UNIQUE   | `AddSearchIndexesAndFilter`             |
| `Reports`         | `IX_Reports_ReviewId`           | Regular  | `AddReports`                            |
| `Reports`         | `IX_Reports_UserId`             | Regular  | `AddReports`                            |

---

## 6. Зведена таблиця CHECK-обмежень

| Таблиця   | Назва обмеження        | Правило                                              |
|-----------|------------------------|------------------------------------------------------|
| `Users`   | `CK_User_Role`         | `Role IN ('user', 'admin', 'moderator')`             |
| `Votes`   | `CK_Vote_Option`       | `SelectedOption IN ('book', 'adaptation', 'movie')`  |
| `Ratings` | `CK_Rating_Book`       | `BookRating >= 0 AND BookRating <= 10`               |
| `Ratings` | `CK_Rating_Adaptation` | `AdaptationRating >= 0 AND AdaptationRating <= 10`   |
| `Reports` | `CK_Report_Status`     | `Status IN ('Pending', 'Resolved', 'Dismissed')`     |

---

## 7. Журнал змін схеми (Migration History)

| Дата       | Міграція                          | Зміни                                                                                    |
|------------|-----------------------------------|------------------------------------------------------------------------------------------|
| 21.04.2026 | `InitialCreate`                   | Початкова схема: всі базові таблиці                                                      |
| 24.04.2026 | `AddVotesAndReviewUpdates`        | `Vote` → `Votes`; додано `IsSpoiler`, `Rating` до `Reviews`; UNIQUE індекс на Votes     |
| 24.04.2026 | `UpdateModelsFix`                 | Порожня міграція (виправлення моделі без змін схеми)                                     |
| 12.05.2026 | `AddFavoritesAndPasswordReset`    | Нові таблиці: `Favorites`, `PasswordResetTokens`                                         |
| 13.05.2026 | `AddSearchIndexesAndFilter`       | `DifferenceMap` → `DifferenceMaps`; `Difference` → `Differences`; перейменування FK     |
| 13.05.2026 | `UpdateSearchIndexesAndFilter`    | Нові індекси: `IX_Works_Title`, `IX_Books_Genre`, `IX_Adaptations_Country`               |
| 17.05.2026 | `AddReports`                      | Нова таблиця: `Reports` з CHECK на `Status`                                              |
| 17.05.2026 | `AddUserAvatar`                   | Додано `AvatarUrl` до `Users`; `Reports.ReviewId` та `UserId` змінено на nullable+SET NULL|

---



*Документ актуалізовано: **17.05.2026**. Відповідає стану `ApplicationDbContextModelSnapshot.cs` після міграції `AddUserAvatar`.*

















