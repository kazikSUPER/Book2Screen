/**
 * Доменні сутності + DTO для API.
 * Узгоджено зі Swagger (Book2Screen API v1).
 *
 * Schemas: LoginDto, RegisterRequest, AuthResponse,
 *   ForgotPasswordRequest, VerifyCodeRequest, ResetPasswordRequest,
 *   BookScreenItemDto, AdaptationDto, DifferenceDto,
 *   UserProfileDto, FavoriteRequest,
 *   ReviewRequest, ReviewResponse, ReportResponse,
 *   VoteRequest, VoteResponse, ProblemDetails.
 */

// ===== Domain entities =====

/** Swagger: DifferenceDto */
export interface DifferencePoint {
  id: string;
  title: string;
  bookText: string;
  filmText: string;
  isSpoiler?: boolean;
}

/** Swagger: BookScreenItemDto */
export interface BookScreenItem {
  id: string;
  title: string;
  year: number;
  genre: string;
  country: string;
  poster: string;
  bookRating: number;
  filmRating: number;
  description: string;
  author?: string;
  filmYear?: number;
  filmCountry?: string;
  filmPoster?: string;
  director?: string;
  bookSummary?: string;
  filmSummary?: string;
  hasMap?: boolean;
  differences?: DifferencePoint[];
  /** Вбудована статистика голосування (бек може повертати разом з твором). */
  voteStats?: VoteResponse;
}

// ===== Auth DTO =====

/** Swagger: LoginDto */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Swagger: RegisterRequest.
 * Бек вимагає пароль: regex `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$`, мін. 8 символів.
 * Нікнейм: 1..50 символів.
 */
export interface RegisterRequest {
  email: string;
  nickname: string;
  password: string;
}

/** Swagger: AuthResponse — спільна відповідь для login / register / reset-password. */
export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  nickname: string;
  /** 'user' | 'admin' | 'moderator' — використовуємо для route-guard на /admin. */
  role?: string;
}

// Backward-compat для існуючого коду.
export type LoginResponse = AuthResponse;
export type RegisterResponse = AuthResponse;

// ── Password reset (3-step flow) ─────

/** Swagger: ForgotPasswordRequest — крок 1: запит коду. */
export interface ForgotPasswordRequest {
  email: string;
}

/** Swagger: VerifyCodeRequest — крок 2: перевірити код. */
export interface VerifyCodeRequest {
  email: string;
  code: string;
}

/** Swagger: ResetPasswordRequest — крок 3: встановити новий пароль (мін. 6). */
export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
}

// Backward-compat.
export type PasswordResetConfirmRequest = VerifyCodeRequest;
export type PasswordResetConfirmResponse = AuthResponse;

// ===== Vote DTO =====

/** ВАЖЛИВО: бек чекає lowercase 'book' | 'movie' (не 'BOOK'/'MOVIE'). */
export type VoteType = 'book' | 'movie';

/** Swagger: VoteRequest */
export interface VoteRequest {
  workId: string;
  voteType: VoteType;
}

/** Swagger: VoteResponse */
export interface VoteResponse {
  workId: string;
  totalVotes: number;
  bookVotes: number;
  movieVotes: number;
  bookPercentage: number;
  moviePercentage: number;
}

// ===== Review DTO =====

/** Бек: 'book' | 'adaptation' | 'comparison'. */
export type ReviewTargetType = 'book' | 'adaptation' | 'comparison';

/** Swagger: ReviewRequest. Текст 10..2000 символів, rating 0..10. */
export interface ReviewRequest {
  workId: string;
  text: string;
  isSpoiler: boolean;
  rating: number;
  targetType: ReviewTargetType;
}

/** Swagger: ReviewResponse */
export interface ReviewResponse {
  reviewId: string;
  workId: string;
  userId: string;
  /** historical поле — бек не завжди повертає, лишаємо для UI. */
  userNickname?: string;
  text: string;
  isSpoiler: boolean;
  rating: number;
  targetType?: ReviewTargetType;
  createdAt: string;
}

// ===== Report DTO =====

/** Swagger: ReportResponse */
export interface ReportResponse {
  reportId: string;
  reviewId: string;
  userId: string;
  reason: string;
  status: string;
  createdAt: string;
  /** Текст відгуку, на який скаржаться (для зручності модерації). */
  reviewText?: string;
}

// ===== User Profile DTO =====

/** Swagger: UserProfileDto */
export interface UserProfileDto {
  username: string;
  email: string;
  avatarUrl?: string;
  /** ISO date-time. */
  joinedAt: string;
}

// ===== Favorites DTO =====

/** Swagger: FavoriteRequest */
export interface FavoriteRequest {
  workId: string;
  kind?: string;
}

// ===== Admin: Adaptation DTO =====

/**
 * Swagger: AdaptationDto.
 * Окрема сутність для адмінської CRUD на /admin/adaptations.
 * type — 'movie' | 'series' | 'anime'.
 */
export interface AdaptationDto {
  id?: string;
  title: string;
  type: string;
  description?: string;
  releaseYear?: number;
  durationMinutes?: number;
  posterUrl?: string;
  studio?: string;
  country?: string;
}

// ===== ProblemDetails =====

/** Swagger: ProblemDetails (RFC 7807). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  /** Поля для нашого власного формату, якщо бекенд віддасть. */
  errorCode?: string;
  message?: string;
  path?: string;
  timestamp?: string;
}

// Backward-compat для існуючих імпортів.
export type ApiError = ProblemDetails;
