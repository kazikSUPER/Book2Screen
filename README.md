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


