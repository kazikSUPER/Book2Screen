using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookScreenExplorer.Infrastructure.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE users (
    id uuid PRIMARY KEY,
    username varchar(50) NOT NULL UNIQUE,
    email varchar(255) NOT NULL UNIQUE,
    password_hash text NOT NULL,
    role varchar(20) NOT NULL,
    registration_date timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active boolean NOT NULL DEFAULT true,
    CHECK (role IN ('user', 'admin', 'moderator'))
);

CREATE TABLE authors (
    id uuid PRIMARY KEY,
    full_name varchar(150) NOT NULL,
    birth_date date,
    nationality varchar(100),
    biography text
);

CREATE TABLE books (
    id uuid PRIMARY KEY,
    title varchar(255) NOT NULL,
    description text,
    genre varchar(100),
    publication_year integer,
    language varchar(50),
    cover_image_url text,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK (publication_year IS NULL OR publication_year > 0)
);

CREATE TABLE adaptations (
    id uuid PRIMARY KEY,
    title varchar(255) NOT NULL,
    type varchar(20) NOT NULL,
    description text,
    release_year integer,
    duration_minutes integer,
    poster_url text,
    studio varchar(150),
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK (type IN ('movie', 'series')),
    CHECK (release_year IS NULL OR release_year > 0),
    CHECK (duration_minutes IS NULL OR duration_minutes > 0)
);

CREATE TABLE actors (
    id uuid PRIMARY KEY,
    full_name varchar(150) NOT NULL,
    birth_date date,
    nationality varchar(100),
    biography text
);

CREATE TABLE works (
    id uuid PRIMARY KEY,
    book_id uuid NOT NULL UNIQUE,
    adaptation_id uuid NOT NULL UNIQUE,
    title varchar(255) NOT NULL,
    summary text,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_works_book FOREIGN KEY (book_id) REFERENCES books(id) ON DELETE CASCADE,
    CONSTRAINT fk_works_adaptation FOREIGN KEY (adaptation_id) REFERENCES adaptations(id) ON DELETE CASCADE
);

CREATE TABLE book_authors (
    id uuid PRIMARY KEY,
    book_id uuid NOT NULL,
    author_id uuid NOT NULL,
    CONSTRAINT uq_book_authors UNIQUE (book_id, author_id),
    CONSTRAINT fk_book_authors_book FOREIGN KEY (book_id) REFERENCES books(id) ON DELETE CASCADE,
    CONSTRAINT fk_book_authors_author FOREIGN KEY (author_id) REFERENCES authors(id) ON DELETE CASCADE
);

CREATE TABLE adaptation_actors (
    id uuid PRIMARY KEY,
    adaptation_id uuid NOT NULL,
    actor_id uuid NOT NULL,
    role_name varchar(150),
    CONSTRAINT uq_adaptation_actors UNIQUE (adaptation_id, actor_id, role_name),
    CONSTRAINT fk_adaptation_actors_adaptation FOREIGN KEY (adaptation_id) REFERENCES adaptations(id) ON DELETE CASCADE,
    CONSTRAINT fk_adaptation_actors_actor FOREIGN KEY (actor_id) REFERENCES actors(id) ON DELETE CASCADE
);

CREATE TABLE reviews (
    id uuid PRIMARY KEY,
    user_id uuid,
    work_id uuid NOT NULL,
    target_type varchar(20) NOT NULL,
    text text NOT NULL,
    likes_count integer NOT NULL DEFAULT 0,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp,
    CONSTRAINT fk_reviews_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_reviews_work FOREIGN KEY (work_id) REFERENCES works(id) ON DELETE CASCADE,
    CHECK (target_type IN ('book', 'adaptation', 'comparison')),
    CHECK (likes_count >= 0)
);

CREATE TABLE votes (
    id uuid PRIMARY KEY,
    user_id uuid,
    work_id uuid NOT NULL,
    selected_option varchar(20) NOT NULL,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_votes_user_work UNIQUE (user_id, work_id),
    CONSTRAINT fk_votes_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_votes_work FOREIGN KEY (work_id) REFERENCES works(id) ON DELETE CASCADE,
    CHECK (selected_option IN ('book', 'adaptation'))
);

CREATE TABLE ratings (
    id uuid PRIMARY KEY,
    work_id uuid NOT NULL UNIQUE,
    book_rating numeric(3,2),
    adaptation_rating numeric(3,2),
    votes_count integer NOT NULL DEFAULT 0,
    updated_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ratings_work FOREIGN KEY (work_id) REFERENCES works(id) ON DELETE CASCADE,
    CHECK (book_rating IS NULL OR (book_rating >= 0 AND book_rating <= 10)),
    CHECK (adaptation_rating IS NULL OR (adaptation_rating >= 0 AND adaptation_rating <= 10)),
    CHECK (votes_count >= 0)
);

CREATE TABLE plot_events (
    id uuid PRIMARY KEY,
    work_id uuid NOT NULL,
    source_type varchar(20) NOT NULL,
    title varchar(255) NOT NULL,
    description text,
    sequence_number integer NOT NULL,
    episode_number integer,
    season_number integer,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_plot_events_work FOREIGN KEY (work_id) REFERENCES works(id) ON DELETE CASCADE,
    CHECK (source_type IN ('book', 'adaptation')),
    CHECK (sequence_number > 0),
    CHECK (episode_number IS NULL OR episode_number > 0),
    CHECK (season_number IS NULL OR season_number > 0)
);

CREATE TABLE difference_maps (
    id uuid PRIMARY KEY,
    work_id uuid NOT NULL UNIQUE,
    title varchar(255) NOT NULL,
    version integer NOT NULL DEFAULT 1,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_difference_maps_work FOREIGN KEY (work_id) REFERENCES works(id) ON DELETE CASCADE,
    CHECK (version > 0)
);

CREATE TABLE differences (
    id uuid PRIMARY KEY,
    map_id uuid NOT NULL,
    book_event_id uuid,
    adaptation_event_id uuid,
    difference_type varchar(20) NOT NULL,
    description text NOT NULL,
    importance_level varchar(20) NOT NULL,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_differences_map FOREIGN KEY (map_id) REFERENCES difference_maps(id) ON DELETE CASCADE,
    CONSTRAINT fk_differences_book_event FOREIGN KEY (book_event_id) REFERENCES plot_events(id) ON DELETE SET NULL,
    CONSTRAINT fk_differences_adaptation_event FOREIGN KEY (adaptation_event_id) REFERENCES plot_events(id) ON DELETE SET NULL,
    CHECK (difference_type IN ('changed', 'added', 'removed')),
    CHECK (importance_level IN ('low', 'medium', 'high')),
    CHECK (book_event_id IS NOT NULL OR adaptation_event_id IS NOT NULL)
);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP TABLE IF EXISTS differences;
DROP TABLE IF EXISTS difference_maps;
DROP TABLE IF EXISTS plot_events;
DROP TABLE IF EXISTS ratings;
DROP TABLE IF EXISTS votes;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS adaptation_actors;
DROP TABLE IF EXISTS book_authors;
DROP TABLE IF EXISTS works;
DROP TABLE IF EXISTS actors;
DROP TABLE IF EXISTS adaptations;
DROP TABLE IF EXISTS books;
DROP TABLE IF EXISTS authors;
DROP TABLE IF EXISTS users;
");
    }
}
