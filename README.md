# Book ↔ Screen Explorer — Крок 2 (Database Migrations)

Готовий шаблон для ролі **DB Developer** на стеку **C# 12 / .NET 8 / EF Core / PostgreSQL 16**.

## Що входить
- `ApplicationDbContext` з усіма сутностями
- Fluent API конфігурації для зв'язків, `CHECK`, `UNIQUE`, `CASCADE`, `SET NULL`
- міграція `Init`
- `down`-логіка через `Down()` у міграції
- `SeedDataExtensions` для автоматичного заповнення початковими даними
- `docker-compose.yml` для PostgreSQL 16
- приклад `Program.cs`, де міграції та seed виконуються автоматично при старті

## Команди
```bash
# 1. Підняти PostgreSQL у Docker
docker compose up -d

# 2. Встановити EF CLI (один раз)
dotnet tool install --global dotnet-ef

# 3. Відновити пакети
dotnet restore

# 4. Застосувати міграції
# якщо ви інтегруєте ці файли у своє рішення:
dotnet ef database update --project BookScreenExplorer.Infrastructure --startup-project BookScreenExplorer.Api
```

## Автоматичне застосування при старті
У `Program.cs` уже показано, як виконати:
1. `context.Database.Migrate()`
2. `await context.SeedAsync()`

Тобто після запуску API база:
- створюється/оновлюється міграціями
- наповнюється тестовими даними

## Database Architecture

База даних спроєктована на основі ER-діаграми та реалізована через ORM Entity Framework Core.

### Основні сутності:

* **Users** – користувачі системи
* **Authors** – автори книг
* **Books** – книги
* **Adaptations** – екранізації
* **Actors** – актори
* **Works** – узагальнена сутність твору
* **Reviews** – відгуки користувачів
* **Ratings** – оцінки
* **Votes** – голоси за відгуки
* **PlotEvents** – події сюжету
* **DifferenceMaps / Differences** – відмінності між книгою та екранізацією

### Типи зв’язків:

* One-to-Many (наприклад, User → Reviews)
* Many-to-Many через проміжні таблиці (BookAuthors, AdaptationActors)

### Політики видалення:

* `ON DELETE CASCADE` – для залежних записів
* `ON DELETE SET NULL` – для збереження історичних даних

Це забезпечує цілісність даних відповідно до ACID-принципів.


## Database Migrations

У проєкті використовується **Entity Framework Core Migrations**.

### Принцип роботи:

* База даних **не створюється вручну**
* Вся схема визначається через код (`ApplicationDbContext`)
* Міграції версіонують структуру БД

### Початкова міграція:

* `Init` – створює всі таблиці відповідно до ER-діаграми
* містить методи:

  * `Up()` – створення таблиць
  * `Down()` – rollback (видалення таблиць)

### Команди:

```bash
dotnet ef migrations add Init
dotnet ef database update
```




## Database Initialization

База даних автоматично ініціалізується при запуску застосунку:

```csharp
await context.Database.MigrateAsync();
await context.SeedAsync();
```

### Що відбувається:

1. Якщо БД не існує — вона створюється
2. Якщо є нові міграції — вони застосовуються
3. БД наповнюється тестовими даними

Це дозволяє повністю автоматизувати розгортання системи.



## Seed Data

Для тестування системи реалізовано початкове наповнення БД.

### Додаються:

* 3–10 користувачів
* автори та книги
* екранізації
* відгуки та рейтинги

Це дозволяє одразу тестувати API без ручного заповнення бази.




## Run Database

Запуск PostgreSQL через Docker:

```bash
docker-compose up -d
```

Після цього:

```bash
dotnet run
```

База даних буде:

* створена автоматично
* змінена через міграції
* наповнена seed-даними

