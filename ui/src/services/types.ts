// ===== Доменні сутності (Domain Entities) =====

/**
 * Точка інтерактивної карти відмінностей між книгою і екранізацією.
 * SCRUM-68 (US 3.2 Book Details).
 */
export interface DifferencePoint {
  id: string;
  // Заголовок сцени, наприклад "Перше знайомство з Драко Малфоєм".
  title: string;
  // Текст для колонки "Книга".
  bookText: string;
  // Текст для колонки "Екранізація".
  filmText: string;
  // Чи позначена точка як спойлер. Тоді текст показуємо blur'ом.
  isSpoiler?: boolean;
}

/**
 * Твір (книга + її екранізація) — основна сутність Book2Screen.
 *
 * Більшість полів описують саму книгу (year, country тощо).
 * Поля з префіксом film* — атрибути екранізації, коли вони відрізняються.
 * Якщо filmYear/filmCountry не задані — на UI вживаємо year/country.
 */
export interface BookScreenItem {
  id: string;
  title: string;
  // Рік видання книги
  year: number;
  genre: string;
  country: string;
  poster: string;
  bookRating: number;
  filmRating: number;
  description: string;

  // Автор книги (для картки книги).
  author?: string;

  // Атрибути екранізації, якщо відрізняються від книги.
  filmYear?: number;
  filmCountry?: string;
  filmPoster?: string;
  director?: string;

  // Окремі короткі описи (book/film). Якщо не задано — fallback на description.
  bookSummary?: string;
  filmSummary?: string;

  // SCRUM-67: фільтр "Лише з картою відмінностей" (true якщо differences не пустий).
  hasMap?: boolean;

  // SCRUM-68: інтерактивна карта відмінностей (опційно).
  differences?: DifferencePoint[];

  // Статистика голосувань (UC-04).
  voteStats?: VoteResponse;
}

// ===== Auth DTO =====

export interface RegisterRequest {
  email: string;
  nickname: string;
  password: string;
}

export interface RegisterResponse {
  token: string;
  userId: string;
  email: string;
  nickname: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  nickname: string;
  role: string;
}

// POST /api/v1/auth/password-reset/confirm
export interface PasswordResetConfirmRequest {
  email: string;
  code: string;
  newPassword: string;
}

// Якщо бекенд повертає токен після підтвердження — відразу авторизуємо.
// Якщо ні (тільки 200 OK) — треба явно викликати login() після.
export interface PasswordResetConfirmResponse {
  token: string;
  userId: string;
  email: string;
  nickname: string;
  role: string;
}

// ===== Vote DTO (UC-04) =====

export type VoteType = 'BOOK' | 'MOVIE';

export interface VoteRequest {
  workId: string;
  voteType: VoteType;
}

export interface VoteResponse {
  workId: string;
  totalVotes: number;
  bookVotes: number;
  movieVotes: number;
  bookPercentage: number;
  moviePercentage: number;
}

// ===== Review DTO (UC-05) =====

export interface ReviewRequest {
  workId: string;
  text: string;
  isSpoiler: boolean;
  rating: number;
  targetType?: string;
}

export interface ReviewResponse {
  reviewId: string;
  workId: string;
  userId: string;
  userNickname?: string;
  text: string;
  isSpoiler: boolean;
  rating: number;
  createdAt: string;
}

// ===== Global Error Schema =====

export interface ApiError {
  timestamp: string;
  errorCode: string;
  message: string;
  path: string;
  details?: Array<{ field: string; issue: string }>;
}
