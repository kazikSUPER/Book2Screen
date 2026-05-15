-- =====================================================
-- Data Integrity Tests for Book2Screen Database
-- Stage 4: DB Engineer Integrity Check
-- =====================================================

-- Purpose:
-- This file contains negative test cases for checking
-- database constraints, unique indexes and foreign keys.
--
-- These queries are expected to fail.
-- If they fail, it means the database constraints work correctly.

-- =====================================================
-- 1. Invalid book rating
-- Expected result:
-- ERROR because book_rating must be between 0 and 10
-- =====================================================

INSERT INTO ratings (
    "Id",
    "WorkId",
    "BookRating",
    "AdaptationRating",
    "VotesCount",
    "UpdatedAt"
)
VALUES (
    '10101010-1010-1010-1010-101010101010',
    '77777777-7777-7777-7777-777777777777',
    15.00,
    8.50,
    1,
    CURRENT_TIMESTAMP
);


-- =====================================================
-- 2. Invalid adaptation rating
-- Expected result:
-- ERROR because adaptation_rating must be between 0 and 10
-- =====================================================

INSERT INTO ratings (
    "Id",
    "WorkId",
    "BookRating",
    "AdaptationRating",
    "VotesCount",
    "UpdatedAt"
)
VALUES (
    '20202020-2020-2020-2020-202020202020',
    '77777777-7777-7777-7777-777777777777',
    9.00,
    -1.00,
    1,
    CURRENT_TIMESTAMP
);


-- =====================================================
-- 3. Invalid vote option
-- Expected result:
-- ERROR because selected_option can only be:
-- 'book' or 'adaptation'
-- =====================================================

INSERT INTO votes (
    "Id",
    "UserId",
    "WorkId",
    "SelectedOption",
    "CreatedAt"
)
VALUES (
    '30303030-3030-3030-3030-303030303030',
    '22222222-2222-2222-2222-222222222222',
    '77777777-7777-7777-7777-777777777777',
    'both',
    CURRENT_TIMESTAMP
);


-- =====================================================
-- 4. Duplicate favorite
-- Expected result:
-- ERROR because one user cannot add the same work
-- to favorites more than once.
--
-- Related unique index:
-- IX_favorites_UserId_WorkId
-- =====================================================

INSERT INTO favorites (
    "Id",
    "UserId",
    "WorkId"
)
VALUES (
    '40404040-4040-4040-4040-404040404040',
    '22222222-2222-2222-2222-222222222222',
    '77777777-7777-7777-7777-777777777777'
);

INSERT INTO favorites (
    "Id",
    "UserId",
    "WorkId"
)
VALUES (
    '50505050-5050-5050-5050-505050505050',
    '22222222-2222-2222-2222-222222222222',
    '77777777-7777-7777-7777-777777777777'
);


-- =====================================================
-- 5. Invalid foreign key: favorite with non-existing user
-- Expected result:
-- ERROR because UserId does not exist in users table
-- =====================================================

INSERT INTO favorites (
    "Id",
    "UserId",
    "WorkId"
)
VALUES (
    '60606060-6060-6060-6060-606060606060',
    '99999999-0000-0000-0000-999999999999',
    '77777777-7777-7777-7777-777777777777'
);


-- =====================================================
-- 6. Invalid foreign key: favorite with non-existing work
-- Expected result:
-- ERROR because WorkId does not exist in works table
-- =====================================================

INSERT INTO favorites (
    "Id",
    "UserId",
    "WorkId"
)
VALUES (
    '70707070-7070-7070-7070-707070707070',
    '22222222-2222-2222-2222-222222222222',
    '99999999-0000-0000-0000-999999999999'
);


-- =====================================================
-- 7. Invalid difference type
-- Expected result:
-- ERROR because difference_type can only be:
-- 'changed', 'added', 'removed'
-- =====================================================

INSERT INTO differences (
    "Id",
    "MapId",
    "BookEventId",
    "AdaptationEventId",
    "DifferenceType",
    "Description",
    "ImportanceLevel",
    "CreatedAt"
)
VALUES (
    '80808080-8080-8080-8080-808080808080',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    'dddddddd-dddd-dddd-dddd-dddddddddddd',
    'incorrect_type',
    'Invalid difference type test.',
    'medium',
    CURRENT_TIMESTAMP
);


-- =====================================================
-- 8. Invalid importance level
-- Expected result:
-- ERROR because importance_level can only be:
-- 'low', 'medium', 'high'
-- =====================================================

INSERT INTO differences (
    "Id",
    "MapId",
    "BookEventId",
    "AdaptationEventId",
    "DifferenceType",
    "Description",
    "ImportanceLevel",
    "CreatedAt"
)
VALUES (
    '90909090-9090-9090-9090-909090909090',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    'dddddddd-dddd-dddd-dddd-dddddddddddd',
    'changed',
    'Invalid importance level test.',
    'critical',
    CURRENT_TIMESTAMP
);


-- =====================================================
-- 9. Invalid difference without related events
-- Expected result:
-- ERROR because at least one of these fields must exist:
-- BookEventId or AdaptationEventId
-- =====================================================

INSERT INTO differences (
    "Id",
    "MapId",
    "BookEventId",
    "AdaptationEventId",
    "DifferenceType",
    "Description",
    "ImportanceLevel",
    "CreatedAt"
)
VALUES (
    '12121212-1212-1212-1212-121212121212',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    NULL,
    NULL,
    'changed',
    'Invalid difference without events.',
    'medium',
    CURRENT_TIMESTAMP
);
