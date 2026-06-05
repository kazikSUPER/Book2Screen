-- ============================================================
--  Book2Screen — Demo Snapshot
--  Generated: 2026-06-05
--  PostgreSQL 15 (Alpine)
--  Usage: psql -U postgres -d book2screen -f book2screen_demo_snapshot_v1.sql
-- ============================================================

-- ── Extensions ───────────────────────────────────────────────
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ── Drop existing tables (safe re-run) ───────────────────────
DROP TABLE IF EXISTS "Differences"          CASCADE;
DROP TABLE IF EXISTS "DifferenceMaps"       CASCADE;
DROP TABLE IF EXISTS "Reports"              CASCADE;
DROP TABLE IF EXISTS "Favorites"            CASCADE;
DROP TABLE IF EXISTS "PasswordResetTokens"  CASCADE;
DROP TABLE IF EXISTS "Votes"                CASCADE;
DROP TABLE IF EXISTS "Reviews"              CASCADE;
DROP TABLE IF EXISTS "Ratings"              CASCADE;
DROP TABLE IF EXISTS "AdaptationActor"      CASCADE;
DROP TABLE IF EXISTS "book_authors"         CASCADE;
DROP TABLE IF EXISTS "Works"                CASCADE;
DROP TABLE IF EXISTS "Adaptations"          CASCADE;
DROP TABLE IF EXISTS "Books"                CASCADE;
DROP TABLE IF EXISTS "Authors"              CASCADE;
DROP TABLE IF EXISTS "Actors"               CASCADE;
DROP TABLE IF EXISTS "Users"                CASCADE;

-- ── Standalone tables ─────────────────────────────────────────

CREATE TABLE "Actors" (
    "Id"          uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "FullName"    varchar(150) NOT NULL,
    "BirthDate"   timestamptz,
    "Nationality" text,
    "Biography"   text,
    "CreatedAt"   timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"   timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Actors" PRIMARY KEY ("Id")
);

CREATE TABLE "Authors" (
    "Id"          uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "FullName"    varchar(150) NOT NULL,
    "BirthDate"   timestamptz,
    "Nationality" text,
    "Biography"   text,
    "CreatedAt"   timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"   timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Authors" PRIMARY KEY ("Id")
);

CREATE TABLE "Books" (
    "Id"              uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "Title"           varchar(255) NOT NULL,
    "Description"     text,
    "Genre"           text,
    "PublicationYear" integer,
    "Language"        text,
    "CoverImageUrl"   text,
    "CreatedAt"       timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"       timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Books" PRIMARY KEY ("Id")
);

CREATE TABLE "Adaptations" (
    "Id"              uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "Title"           varchar(255) NOT NULL,
    "Type"            varchar(20)  NOT NULL,
    "Description"     text,
    "ReleaseYear"     integer,
    "DurationMinutes" integer,
    "PosterUrl"       text,
    "Studio"          text,
    "Country"         text,
    "CreatedAt"       timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"       timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Adaptations" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id"           uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "Username"     varchar(50)  NOT NULL,
    "Email"        varchar(255) NOT NULL,
    "PasswordHash" text         NOT NULL,
    "Role"         varchar(20)  NOT NULL,
    "IsActive"     boolean      NOT NULL DEFAULT true,
    "AvatarUrl"    varchar(1000),
    "CreatedAt"    timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"    timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Users"    PRIMARY KEY ("Id"),
    CONSTRAINT "CK_User_Role" CHECK ("Role" IN ('user', 'admin', 'moderator'))
);

CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
CREATE UNIQUE INDEX "IX_Users_Email"    ON "Users" ("Email");

CREATE TABLE "PasswordResetTokens" (
    "Id"         uuid        NOT NULL DEFAULT uuid_generate_v4(),
    "Email"      text        NOT NULL,
    "Code"       varchar(10) NOT NULL,
    "ExpiryTime" timestamptz NOT NULL,
    "IsUsed"     boolean     NOT NULL DEFAULT false,
    "CreatedAt"  timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt"  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id")
);

-- ── Dependent tables ──────────────────────────────────────────

CREATE TABLE "book_authors" (
    "AuthorsId" uuid NOT NULL,
    "BooksId"   uuid NOT NULL,
    CONSTRAINT "PK_book_authors" PRIMARY KEY ("AuthorsId", "BooksId"),
    CONSTRAINT "FK_book_authors_Authors_AuthorsId" FOREIGN KEY ("AuthorsId") REFERENCES "Authors" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_book_authors_Books_BooksId"     FOREIGN KEY ("BooksId")   REFERENCES "Books"   ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_book_authors_BooksId" ON "book_authors" ("BooksId");

CREATE TABLE "AdaptationActor" (
    "AdaptationId" uuid         NOT NULL,
    "ActorId"      uuid         NOT NULL,
    "RoleName"     varchar(150) NOT NULL,
    "Id"           uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "CreatedAt"    timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"    timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_AdaptationActor" PRIMARY KEY ("AdaptationId", "ActorId", "RoleName"),
    CONSTRAINT "FK_AdaptationActor_Actors_ActorId"           FOREIGN KEY ("ActorId")      REFERENCES "Actors"      ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AdaptationActor_Adaptations_AdaptationId" FOREIGN KEY ("AdaptationId") REFERENCES "Adaptations" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_AdaptationActor_ActorId" ON "AdaptationActor" ("ActorId");

CREATE TABLE "Works" (
    "Id"           uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "BookId"       uuid         NOT NULL,
    "AdaptationId" uuid         NOT NULL,
    "Title"        varchar(255) NOT NULL,
    "Summary"      text,
    "CreatedAt"    timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt"    timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Works" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Works_Books_BookId"             FOREIGN KEY ("BookId")       REFERENCES "Books"       ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Works_Adaptations_AdaptationId" FOREIGN KEY ("AdaptationId") REFERENCES "Adaptations" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_Works_BookId"       ON "Works" ("BookId");
CREATE UNIQUE INDEX "IX_Works_AdaptationId" ON "Works" ("AdaptationId");
CREATE        INDEX "IX_Works_Title"        ON "Works" ("Title");

CREATE TABLE "Ratings" (
    "Id"               uuid    NOT NULL DEFAULT uuid_generate_v4(),
    "WorkId"           uuid    NOT NULL,
    "BookRating"       decimal NOT NULL,
    "AdaptationRating" decimal NOT NULL,
    "VotesCount"       integer NOT NULL DEFAULT 0,
    "CreatedAt"        timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt"        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Ratings"           PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Ratings_Works_WorkId" FOREIGN KEY ("WorkId") REFERENCES "Works" ("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_Rating_Book"       CHECK ("BookRating"       >= 0 AND "BookRating"       <= 10),
    CONSTRAINT "CK_Rating_Adaptation" CHECK ("AdaptationRating" >= 0 AND "AdaptationRating" <= 10)
);
CREATE UNIQUE INDEX "IX_Ratings_WorkId" ON "Ratings" ("WorkId");

CREATE TABLE "Reviews" (
    "Id"         uuid    NOT NULL DEFAULT uuid_generate_v4(),
    "UserId"     uuid,
    "WorkId"     uuid    NOT NULL,
    "TargetType" text    NOT NULL,
    "Text"       text    NOT NULL,
    "IsSpoiler"  boolean NOT NULL DEFAULT false,
    "Rating"     float8  NOT NULL DEFAULT 0,
    "LikesCount" integer NOT NULL DEFAULT 0,
    "CreatedAt"  timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt"  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reviews_Works_WorkId" FOREIGN KEY ("WorkId") REFERENCES "Works"  ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"  ("Id") ON DELETE SET NULL
);
CREATE INDEX "IX_Reviews_UserId" ON "Reviews" ("UserId");
CREATE INDEX "IX_Reviews_WorkId" ON "Reviews" ("WorkId");

CREATE TABLE "Votes" (
    "Id"             uuid NOT NULL DEFAULT uuid_generate_v4(),
    "UserId"         uuid NOT NULL,
    "WorkId"         uuid NOT NULL,
    "SelectedOption" text NOT NULL,
    "CreatedAt"      timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt"      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Votes"      PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Votes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Votes_Works_WorkId" FOREIGN KEY ("WorkId") REFERENCES "Works" ("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_Vote_Option" CHECK ("SelectedOption" IN ('book', 'adaptation', 'movie'))
);
CREATE UNIQUE INDEX "IX_Votes_UserId_WorkId" ON "Votes" ("UserId", "WorkId");
CREATE        INDEX "IX_Votes_WorkId"        ON "Votes" ("WorkId");

CREATE TABLE "Favorites" (
    "Id"        uuid NOT NULL DEFAULT uuid_generate_v4(),
    "UserId"    uuid NOT NULL,
    "WorkId"    uuid NOT NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Favorites" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Favorites_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Favorites_Works_WorkId" FOREIGN KEY ("WorkId") REFERENCES "Works" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_Favorites_UserId_WorkId" ON "Favorites" ("UserId", "WorkId");
CREATE        INDEX "IX_Favorites_WorkId"         ON "Favorites" ("WorkId");

CREATE TABLE "Reports" (
    "Id"        uuid         NOT NULL DEFAULT uuid_generate_v4(),
    "ReviewId"  uuid,
    "UserId"    uuid,
    "Reason"    varchar(500) NOT NULL,
    "Status"    varchar(20)  NOT NULL,
    "CreatedAt" timestamptz  NOT NULL DEFAULT now(),
    "UpdatedAt" timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Reports"   PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reports_Reviews_ReviewId" FOREIGN KEY ("ReviewId") REFERENCES "Reviews" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Reports_Users_UserId"     FOREIGN KEY ("UserId")   REFERENCES "Users"   ("Id") ON DELETE SET NULL,
    CONSTRAINT "CK_Report_Status" CHECK ("Status" IN ('Pending', 'Resolved', 'Dismissed'))
);
CREATE INDEX "IX_Reports_ReviewId" ON "Reports" ("ReviewId");
CREATE INDEX "IX_Reports_UserId"   ON "Reports" ("UserId");

CREATE TABLE "DifferenceMaps" (
    "Id"        uuid NOT NULL DEFAULT uuid_generate_v4(),
    "WorkId"    uuid NOT NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_DifferenceMaps" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DifferenceMaps_Works_WorkId" FOREIGN KEY ("WorkId") REFERENCES "Works" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_DifferenceMaps_WorkId" ON "DifferenceMaps" ("WorkId");

CREATE TABLE "Differences" (
    "Id"                uuid        NOT NULL DEFAULT uuid_generate_v4(),
    "MapId"             uuid        NOT NULL,
    "BookEventId"       uuid,
    "AdaptationEventId" uuid,
    "DifferenceType"    varchar(20) NOT NULL,
    "Description"       text        NOT NULL,
    "ImportanceLevel"   varchar(20) NOT NULL,
    "CreatedAt"         timestamptz NOT NULL DEFAULT now(),
    "UpdatedAt"         timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Differences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Differences_DifferenceMaps_MapId" FOREIGN KEY ("MapId") REFERENCES "DifferenceMaps" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Differences_MapId"             ON "Differences" ("MapId");
CREATE INDEX "IX_Differences_BookEventId"       ON "Differences" ("BookEventId");
CREATE INDEX "IX_Differences_AdaptationEventId" ON "Differences" ("AdaptationEventId");

-- ── EF Core migrations history ────────────────────────────────
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    varchar(150) NOT NULL,
    "ProductVersion" varchar(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20260421114950_InitialCreate',                  '8.0.0'),
('20260424153950_AddVotesAndReviewUpdates',        '8.0.0'),
('20260424155322_UpdateModelsFix',                 '8.0.0'),
('20260512073329_AddFavoritesAndPasswordReset',    '8.0.0'),
('20260513114527_AddSearchIndexesAndFilter',       '8.0.0'),
('20260513114558_UpdateSearchIndexesAndFilter',    '8.0.0'),
('20260517120423_AddReports',                      '8.0.0'),
('20260517121337_AddUserAvatar',                   '8.0.0')
ON CONFLICT DO NOTHING;

-- ============================================================
--  SEED DATA  (відповідає DbSeeder.cs)
-- ============================================================

-- UUID константи для seed-даних
DO $$
DECLARE
    v_admin_id      uuid := 'a0000000-0000-0000-0000-000000000001';
    v_user_id       uuid := 'a0000000-0000-0000-0000-000000000002';
    v_author_id     uuid := 'b0000000-0000-0000-0000-000000000001';
    v_actor_id      uuid := 'b0000000-0000-0000-0000-000000000002';
    v_book_id       uuid := 'c0000000-0000-0000-0000-000000000001';
    v_adaptation_id uuid := 'c0000000-0000-0000-0000-000000000002';
    v_work_id       uuid := 'd0000000-0000-0000-0000-000000000001';
    v_rating_id     uuid := 'd0000000-0000-0000-0000-000000000002';
    v_review_id     uuid := 'e0000000-0000-0000-0000-000000000001';
    v_report_id     uuid := 'e0000000-0000-0000-0000-000000000002';
BEGIN

-- Users
INSERT INTO "Users" ("Id","Username","Email","PasswordHash","Role","IsActive","AvatarUrl","CreatedAt","UpdatedAt")
VALUES
(
    v_admin_id,
    'admin',
    'admin@book2screen.com',
    -- BCrypt hash of 'Admin123!'
    '$2a$11$KvmMoOjAh/1tGYDMVJVlrOU7S3TaEeHr4Z5HcVBQzFO2VLtSgmEfK',
    'admin',
    true,
    'https://ui-avatars.com/api/?name=Admin&background=random',
    now(), now()
),
(
    v_user_id,
    'john_doe',
    'john@example.com',
    -- BCrypt hash of 'User123!'
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
    'user',
    true,
    'https://ui-avatars.com/api/?name=John+Doe&background=random',
    now(), now()
)
ON CONFLICT DO NOTHING;

-- Author
INSERT INTO "Authors" ("Id","FullName","Nationality","Biography","CreatedAt","UpdatedAt")
VALUES (
    v_author_id,
    'Frank Herbert',
    'American',
    'American science fiction novelist best known for the novel Dune.',
    now(), now()
)
ON CONFLICT DO NOTHING;

-- Actor
INSERT INTO "Actors" ("Id","FullName","Nationality","Biography","CreatedAt","UpdatedAt")
VALUES (
    v_actor_id,
    'Timothée Chalamet',
    'American/French',
    'Academy Award-nominated actor.',
    now(), now()
)
ON CONFLICT DO NOTHING;

-- Book
INSERT INTO "Books" ("Id","Title","Description","Genre","PublicationYear","Language","CreatedAt","UpdatedAt")
VALUES (
    v_book_id,
    'Dune',
    'A story about a young man''s journey to the desert planet Arrakis.',
    'Sci-Fi',
    1965,
    'English',
    now(), now()
)
ON CONFLICT DO NOTHING;

-- book_authors (M:M)
INSERT INTO "book_authors" ("AuthorsId","BooksId")
VALUES (v_author_id, v_book_id)
ON CONFLICT DO NOTHING;

-- Adaptation
INSERT INTO "Adaptations" ("Id","Title","Type","Description","ReleaseYear","DurationMinutes","Studio","Country","PosterUrl","CreatedAt","UpdatedAt")
VALUES (
    v_adaptation_id,
    'Dune: Part One',
    'movie',
    'Denis Villeneuve''s 2021 epic science fiction film.',
    2021,
    155,
    'Legendary Pictures',
    'USA',
    'https://m.media-amazon.com/images/M/MV5BMDQ0NjgyN2YtNWU4Ny00YjZlLWIzMTktMzljZWM2MTgzY2RhXkEyXkFqcGdeQXVyNjU0OTQ0OTY@._V1_.jpg',
    now(), now()
)
ON CONFLICT DO NOTHING;

-- AdaptationActor
INSERT INTO "AdaptationActor" ("AdaptationId","ActorId","RoleName","Id","CreatedAt","UpdatedAt")
VALUES (v_adaptation_id, v_actor_id, 'Paul Atreides', uuid_generate_v4(), now(), now())
ON CONFLICT DO NOTHING;

-- Work
INSERT INTO "Works" ("Id","BookId","AdaptationId","Title","Summary","CreatedAt","UpdatedAt")
VALUES (
    v_work_id,
    v_book_id,
    v_adaptation_id,
    'Dune: Book vs 2021 Movie',
    'A comparison between Frank Herbert''s masterpiece and Villeneuve''s adaptation.',
    now(), now()
)
ON CONFLICT DO NOTHING;

-- Rating
INSERT INTO "Ratings" ("Id","WorkId","BookRating","AdaptationRating","VotesCount","CreatedAt","UpdatedAt")
VALUES (v_rating_id, v_work_id, 9.5, 8.9, 1, now(), now())
ON CONFLICT DO NOTHING;

-- Review
INSERT INTO "Reviews" ("Id","UserId","WorkId","TargetType","Text","IsSpoiler","LikesCount","Rating","CreatedAt","UpdatedAt")
VALUES (
    v_review_id,
    v_user_id,
    v_work_id,
    'comparison',
    'The movie is visually stunning, but the book offers much more world-building. Spoiler: Paul survives!',
    false,
    10,
    8.5,
    now(), now()
)
ON CONFLICT DO NOTHING;

-- Report
INSERT INTO "Reports" ("Id","ReviewId","UserId","Reason","Status","CreatedAt","UpdatedAt")
VALUES (
    v_report_id,
    v_review_id,
    v_admin_id,
    'Contains hidden spoilers without tag',
    'Pending',
    now(), now()
)
ON CONFLICT DO NOTHING;

END $$;

-- ============================================================
--  END OF SNAPSHOT
-- ============================================================
