-- =====================================================
-- Complex SQL Queries for Book2Screen Database
-- Stage 4: DB Engineer Tasks
-- =====================================================

-- 1. JOIN: Отримання повної інформації про твір,
-- книгу, адаптацію та рейтинг
SELECT
    w."Id" AS work_id,
    w."Title" AS work_title,
    b."Title" AS book_title,
    b."Genre" AS book_genre,
    b."PublicationYear" AS publication_year,
    a."Title" AS adaptation_title,
    a."Type" AS adaptation_type,
    a."ReleaseYear" AS release_year,
    r."BookRating" AS book_rating,
    r."AdaptationRating" AS adaptation_rating
FROM works w
JOIN books b ON w."BookId" = b."Id"
JOIN adaptations a ON w."AdaptationId" = a."Id"
LEFT JOIN ratings r ON r."WorkId" = w."Id";


-- 2. GROUP BY + COUNT:
-- Кількість творів за жанрами книг
SELECT
    b."Genre" AS genre,
    COUNT(w."Id") AS works_count
FROM works w
JOIN books b ON w."BookId" = b."Id"
GROUP BY b."Genre"
ORDER BY works_count DESC;


-- 3. AVG:
-- Середній рейтинг книг та адаптацій
SELECT
    ROUND(AVG(r."BookRating"), 2) AS average_book_rating,
    ROUND(AVG(r."AdaptationRating"), 2) AS average_adaptation_rating
FROM ratings r;


-- 4. Фільтрація:
-- Пошук творів, де рейтинг книги вищий за рейтинг адаптації
SELECT
    w."Title" AS work_title,
    r."BookRating" AS book_rating,
    r."AdaptationRating" AS adaptation_rating
FROM works w
JOIN ratings r ON r."WorkId" = w."Id"
WHERE r."BookRating" > r."AdaptationRating";


-- 5. Пошук:
-- Пошук творів за назвою
SELECT
    w."Id",
    w."Title",
    w."Summary"
FROM works w
WHERE LOWER(w."Title") LIKE LOWER('%dune%');


-- 6. Звіт:
-- Звіт по активності користувачів:
-- кількість відгуків, голосів та обраних творів
SELECT
    u."Id" AS user_id,
    u."Username" AS username,
    COUNT(DISTINCT rev."Id") AS reviews_count,
    COUNT(DISTINCT v."Id") AS votes_count,
    COUNT(DISTINCT f."Id") AS favorites_count
FROM users u
LEFT JOIN reviews rev ON rev."UserId" = u."Id"
LEFT JOIN votes v ON v."UserId" = u."Id"
LEFT JOIN favorites f ON f."UserId" = u."Id"
GROUP BY u."Id", u."Username"
ORDER BY reviews_count DESC, votes_count DESC;


-- 7. Звіт:
-- Найпопулярніші твори за кількістю додавань в обране
SELECT
    w."Id" AS work_id,
    w."Title" AS work_title,
    COUNT(f."Id") AS favorites_count
FROM works w
LEFT JOIN favorites f ON f."WorkId" = w."Id"
GROUP BY w."Id", w."Title"
ORDER BY favorites_count DESC;


-- 8. JOIN + фільтрація:
-- Отримання розбіжностей між книгою та адаптацією
SELECT
    w."Title" AS work_title,
    dm."Title" AS difference_map_title,
    d."DifferenceType" AS difference_type,
    d."ImportanceLevel" AS importance_level,
    d."Description" AS description
FROM differences d
JOIN difference_maps dm ON d."MapId" = dm."Id"
JOIN works w ON dm."WorkId" = w."Id"
WHERE d."ImportanceLevel" IN ('medium', 'high')
ORDER BY d."ImportanceLevel" DESC;


-- 9. Звіт:
-- Кількість розбіжностей по кожному твору
SELECT
    w."Title" AS work_title,
    COUNT(d."Id") AS differences_count
FROM works w
JOIN difference_maps dm ON dm."WorkId" = w."Id"
LEFT JOIN differences d ON d."MapId" = dm."Id"
GROUP BY w."Title"
ORDER BY differences_count DESC;


-- 10. Пошук + фільтрація:
-- Пошук книг за жанром
SELECT
    b."Id",
    b."Title",
    b."Genre",
    b."PublicationYear"
FROM books b
WHERE LOWER(b."Genre") = LOWER('Science Fiction')
ORDER BY b."PublicationYear";
