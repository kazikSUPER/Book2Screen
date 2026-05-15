-- =====================================================
-- EXPLAIN Queries for Book2Screen Database
-- Stage 4: DB Engineer Optimization Check
-- =====================================================

-- Purpose:
-- This file contains EXPLAIN queries used to check
-- execution plans for frequently used search, filter,
-- join and report operations.

-- =====================================================
-- 1. Check search by work title
-- Related index: IX_works_Title
-- =====================================================

EXPLAIN
SELECT
    w."Id",
    w."Title",
    w."Summary"
FROM works w
WHERE w."Title" = 'Dune: Book vs Adaptation';


-- =====================================================
-- 2. Check case-insensitive search by work title
-- Related index: IX_works_Title
-- Note:
-- For better optimization of ILIKE / LOWER search,
-- a functional index can be added in the future.
-- =====================================================

EXPLAIN
SELECT
    w."Id",
    w."Title",
    w."Summary"
FROM works w
WHERE LOWER(w."Title") LIKE LOWER('%dune%');


-- =====================================================
-- 3. Check filtering books by genre
-- Related index: IX_books_Genre
-- =====================================================

EXPLAIN
SELECT
    b."Id",
    b."Title",
    b."Genre",
    b."PublicationYear"
FROM books b
WHERE b."Genre" = 'Science Fiction';


-- =====================================================
-- 4. Check filtering adaptations by country
-- Related index: IX_adaptations_Country
-- =====================================================

EXPLAIN
SELECT
    a."Id",
    a."Title",
    a."Country",
    a."ReleaseYear"
FROM adaptations a
WHERE a."Country" = 'USA';


-- =====================================================
-- 5. Check unique favorite lookup
-- Related index: IX_favorites_UserId_WorkId
-- =====================================================

EXPLAIN
SELECT
    f."Id",
    f."UserId",
    f."WorkId"
FROM favorites f
WHERE f."UserId" = '22222222-2222-2222-2222-222222222222'
  AND f."WorkId" = '77777777-7777-7777-7777-777777777777';


-- =====================================================
-- 6. Check JOIN query for work details
-- Related indexes:
-- - PK_books
-- - PK_adaptations
-- - IX_works_BookId
-- - IX_works_AdaptationId
-- =====================================================

EXPLAIN
SELECT
    w."Id" AS work_id,
    w."Title" AS work_title,
    b."Title" AS book_title,
    b."Genre" AS book_genre,
    a."Title" AS adaptation_title,
    a."Type" AS adaptation_type,
    r."BookRating" AS book_rating,
    r."AdaptationRating" AS adaptation_rating
FROM works w
JOIN books b ON w."BookId" = b."Id"
JOIN adaptations a ON w."AdaptationId" = a."Id"
LEFT JOIN ratings r ON r."WorkId" = w."Id";


-- =====================================================
-- 7. Check report query with GROUP BY and COUNT
-- =====================================================

EXPLAIN
SELECT
    b."Genre" AS genre,
    COUNT(w."Id") AS works_count
FROM works w
JOIN books b ON w."BookId" = b."Id"
GROUP BY b."Genre"
ORDER BY works_count DESC;


-- =====================================================
-- 8. Check average ratings report
-- =====================================================

EXPLAIN
SELECT
    ROUND(AVG(r."BookRating"), 2) AS average_book_rating,
    ROUND(AVG(r."AdaptationRating"), 2) AS average_adaptation_rating
FROM ratings r;


-- =====================================================
-- 9. Check difference report
-- =====================================================

EXPLAIN
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


-- =====================================================
-- 10. Check user activity report
-- =====================================================

EXPLAIN
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
