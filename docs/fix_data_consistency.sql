-- =============================================================================
-- DATA CONSISTENCY FIXES — Book2Screen
-- Дата: 05.06.2026
-- Опис: Скрипти виправлення аномалій даних що могли накопичитись
--       в результаті багів бізнес-логіки, виявлених аналізом DbSeeder
--       та міграційної історії.
-- Запуск: psql -U $DB_USER -d $DB_NAME -f fix_data_consistency.sql
-- ВАЖЛИВО: Запускати в транзакції. Перевірити SELECT перед COMMIT.
-- =============================================================================

BEGIN;

-- =============================================================================
-- FIX 1: Reviews з IsSpoiler = false, але текст містить спойлери
-- -----------------------------------------------------------------------------
-- Причина бага: У DbSeeder review створюється з IsSpoiler = false,
-- але текст явно містить "Spoiler:". Модератор мав би позначити через
-- ModerateReviewAsync("spoiler"), але якщо цього не сталось —
-- такі відгуки залишаються непозначеними.
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT Id, Text, IsSpoiler FROM "Reviews"
-- WHERE "IsSpoiler" = false
--   AND ("Text" ILIKE '%spoiler%' OR "Text" ILIKE 'spoiler:%');

UPDATE "Reviews"
SET
    "IsSpoiler" = true,
    "UpdatedAt" = NOW()
WHERE
    "IsSpoiler" = false
    AND (
        "Text" ILIKE '%spoiler:%'
        OR "Text" ILIKE '%[spoiler]%'
        OR "Text" ILIKE 'spoiler!%'
    );

-- =============================================================================
-- FIX 2: Reports зі статусом 'Pending' на вже видалені Reviews
-- -----------------------------------------------------------------------------
-- Причина бага: При видаленні Review через ModerateReviewAsync("approve")
-- ReviewId в Reports стає NULL (ON DELETE SET NULL), але Status
-- залишається 'Pending' — звіт висить у черзі модерації назавжди.
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT Id, "ReviewId", "Status" FROM "Reports"
-- WHERE "ReviewId" IS NULL AND "Status" = 'Pending';

UPDATE "Reports"
SET
    "Status" = 'Resolved',
    "UpdatedAt" = NOW()
WHERE
    "ReviewId" IS NULL
    AND "Status" = 'Pending';

-- =============================================================================
-- FIX 3: Votes з SelectedOption = 'movie' — застарілий варіант
-- -----------------------------------------------------------------------------
-- Причина бага: Міграція AddVotesAndReviewUpdates встановила CHECK:
-- SelectedOption IN ('book', 'adaptation', 'movie').
-- Але в коді VotesController фігурують лише 'book' та 'adaptation'.
-- Значення 'movie' є дублем 'adaptation' — залишилось як legacy.
-- Нормалізуємо до актуального значення.
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT COUNT(*) FROM "Votes" WHERE "SelectedOption" = 'movie';

UPDATE "Votes"
SET
    "SelectedOption" = 'adaptation',
    "UpdatedAt" = NOW()
WHERE
    "SelectedOption" = 'movie';

-- =============================================================================
-- FIX 4: Ratings з VotesCount = 0 але BookRating / AdaptationRating != NULL
-- -----------------------------------------------------------------------------
-- Причина бага: Seed створює Rating з VotesCount = 1, але якщо
-- дані скидались і пересівались — могла виникнути ситуація де
-- рейтинг є, а лічильник голосів = 0 (некоректний стан агрегату).
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT Id, "WorkId", "BookRating", "AdaptationRating", "VotesCount"
-- FROM "Ratings"
-- WHERE "VotesCount" = 0
--   AND ("BookRating" IS NOT NULL OR "AdaptationRating" IS NOT NULL);

UPDATE "Ratings"
SET
    "BookRating" = NULL,
    "AdaptationRating" = NULL,
    "UpdatedAt" = NOW()
WHERE
    "VotesCount" = 0
    AND ("BookRating" IS NOT NULL OR "AdaptationRating" IS NOT NULL);

-- =============================================================================
-- FIX 5: Favorites — дублікати (UserId + WorkId)
-- -----------------------------------------------------------------------------
-- Причина бага: До міграції AddFavoritesAndPasswordReset UNIQUE індекс
-- на (UserId, WorkId) міг бути відсутній у ранніх версіях схеми,
-- що дозволяло додавати один твір в обране двічі.
-- Залишаємо лише найстаріший запис (перше додавання).
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT "UserId", "WorkId", COUNT(*) FROM "Favorites"
-- GROUP BY "UserId", "WorkId" HAVING COUNT(*) > 1;

DELETE FROM "Favorites"
WHERE "Id" IN (
    SELECT "Id"
    FROM (
        SELECT
            "Id",
            ROW_NUMBER() OVER (
                PARTITION BY "UserId", "WorkId"
                ORDER BY "CreatedAt" ASC
            ) AS rn
        FROM "Favorites"
    ) ranked
    WHERE rn > 1
);

-- =============================================================================
-- FIX 6: PasswordResetTokens — протерміновані невикористані токени
-- -----------------------------------------------------------------------------
-- Причина бага: Токени після закінчення терміну дії залишаються в БД
-- і засмічують таблицю. Не є критичним багом, але впливає на
-- продуктивність запиту verify-code (IX_PasswordResetTokens_Email_IsUsed).
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT COUNT(*) FROM "PasswordResetTokens"
-- WHERE "IsUsed" = false AND "ExpiryTime" < NOW();

DELETE FROM "PasswordResetTokens"
WHERE
    "IsUsed" = false
    AND "ExpiryTime" < NOW();

-- =============================================================================
-- FIX 7: Users без жодного запису активності (IsActive = true але orphaned)
-- -----------------------------------------------------------------------------
-- Причина бага: Якщо реєстрація падала після створення User але до
-- збереження пов'язаних даних — міг залишитись "порожній" акаунт.
-- Позначаємо неактивними users які старші 7 днів і не мають жодної активності.
-- -----------------------------------------------------------------------------
-- Перегляд перед виправленням:
-- SELECT u."Id", u."Email", u."CreatedAt" FROM "Users" u
-- WHERE u."IsActive" = true
--   AND u."CreatedAt" < NOW() - INTERVAL '7 days'
--   AND NOT EXISTS (SELECT 1 FROM "Reviews" r WHERE r."UserId" = u."Id")
--   AND NOT EXISTS (SELECT 1 FROM "Votes"   v WHERE v."UserId" = u."Id")
--   AND NOT EXISTS (SELECT 1 FROM "Favorites" f WHERE f."UserId" = u."Id")
--   AND u."Role" = 'user';

UPDATE "Users"
SET
    "IsActive" = false,
    "UpdatedAt" = NOW()
WHERE
    "IsActive" = true
    AND "CreatedAt" < NOW() - INTERVAL '7 days'
    AND "Role" = 'user'
    AND NOT EXISTS (SELECT 1 FROM "Reviews"   r WHERE r."UserId" = "Users"."Id")
    AND NOT EXISTS (SELECT 1 FROM "Votes"     v WHERE v."UserId" = "Users"."Id")
    AND NOT EXISTS (SELECT 1 FROM "Favorites" f WHERE f."UserId" = "Users"."Id");

-- =============================================================================
-- ПЕРЕВІРКА ПІСЛЯ ВИПРАВЛЕНЬ
-- Розкоментуй і виконай перед COMMIT щоб впевнитись у коректності змін.
-- =============================================================================

-- SELECT 'FIX1 - Spoiler reviews fixed' AS check, COUNT(*) AS count
--   FROM "Reviews" WHERE "IsSpoiler" = true AND "Text" ILIKE '%spoiler:%';

-- SELECT 'FIX2 - Orphaned pending reports' AS check, COUNT(*) AS count
--   FROM "Reports" WHERE "ReviewId" IS NULL AND "Status" = 'Pending';

-- SELECT 'FIX3 - Legacy movie votes remaining' AS check, COUNT(*) AS count
--   FROM "Votes" WHERE "SelectedOption" = 'movie';

-- SELECT 'FIX4 - Zero-vote ratings with values' AS check, COUNT(*) AS count
--   FROM "Ratings" WHERE "VotesCount" = 0
--   AND ("BookRating" IS NOT NULL OR "AdaptationRating" IS NOT NULL);

-- SELECT 'FIX5 - Duplicate favorites remaining' AS check, COUNT(*) AS count
--   FROM (SELECT "UserId", "WorkId", COUNT(*) c FROM "Favorites"
--         GROUP BY "UserId", "WorkId" HAVING COUNT(*) > 1) x;

-- SELECT 'FIX6 - Expired tokens remaining' AS check, COUNT(*) AS count
--   FROM "PasswordResetTokens" WHERE "IsUsed" = false AND "ExpiryTime" < NOW();

-- SELECT 'FIX7 - Inactive orphaned users' AS check, COUNT(*) AS count
--   FROM "Users" WHERE "IsActive" = false AND "Role" = 'user';

-- =============================================================================
COMMIT;
-- У разі проблем: ROLLBACK;
-- =============================================================================
