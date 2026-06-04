# Інструкція з розгортання Book2Screen

Цей документ містить інструкції щодо розгортання бекенду та фронтенду проекту Book2Screen за допомогою Docker та GitHub Actions.

## 📋 Системні вимоги
- Docker та Docker Compose.
- Акаунт на Docker Hub.
- База даних PostgreSQL (рекомендовано Supabase).
- GitHub репозиторій для налаштування CI/CD.

## 🛠 Налаштування GitHub Secrets
Для роботи автоматичного розгортання (CD) необхідно додати наступні секрети у вашому репозиторії (`Settings > Secrets and variables > Actions`):

| Секрет | Опис | Приклад |
| :--- | :--- | :--- |
| `DOCKERHUB_USERNAME` | Ваше ім'я користувача на Docker Hub | `myuser` |
| `DOCKERHUB_TOKEN` | Access Token для Docker Hub | `dckr_pat_...` |
| `DB_CONNECTION_STRING` | Рядок підключення до PostgreSQL | `Host=...;Database=...;Username=...;Password=...` |
| `JWT_SECRET` | Секретний ключ для JWT токенів | `SuperSecretKey123!` |
| `JWT_ISSUER` | Видавець токена | `Book2ScreenAPI` |
| `JWT_AUDIENCE` | Аудиторія токена | `Book2ScreenClient` |
| `JWT_EXPIRY_MINUTES` | Час життя токена у хвилинах | `60` |
| `ALLOWED_ORIGINS` | Дозволені домени для CORS (через кому) | `https://my-app.vercel.app` |

## 🚀 Розгортання через CI/CD (Автоматично)
1. **CI Pipeline**: Кожен Pull Request до гілок `main` або `develop` запускає автоматичне збирання та тестування.
2. **CD Pipeline**: Кожен Push у гілку `main` автоматично збирає Docker-образи та завантажує їх на Docker Hub.

## 🐳 Локальне розгортання через Docker
Якщо ви хочете запустити проект локально у Docker-контейнерах:

1. Створіть файл `.env` у корені проекту на основі `.env.example`.
2. Виконайте команду:
   ```bash
   docker-compose up --build
   ```

## 🔍 Перевірка стану (Health Checks)
Після розгортання ви можете перевірити стан системи за адресою:
`https://<your-api-domain>/health`

Він покаже статус підключення до бази даних та готовність API.

## 🛡 Безпека та Обмеження
- **Rate Limiting**: Впроваджено обмеження: 100 запитів/хв глобально та 10 запитів/хв для авторизації.
- **CORS**: Обмежено доменами, вказаними у `ALLOWED_ORIGINS`.
