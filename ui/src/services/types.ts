// ===== Доменні сутності (Domain Entities) =====

// Твір (книга + її екранізація) — основна сутність Book2Screen.
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
}

// ===== Auth DTO =====

// Запит на реєстрацію нового користувача.
// Endpoint: POST /api/v1/auth/register
export interface RegisterRequest {
  email: string;
  nickname: string;
  password: string;
}

// Відповідь на успішну реєстрацію або вхід.
export interface RegisterResponse {
  token: string;
  userId: string;
  email: string;
  nickname: string;
  role: string;
}

// Запит на авторизацію.
// Endpoint: POST /api/v1/auth/login
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Відповідь із JWT-токеном (така ж як при реєстрації).
 */
export interface LoginResponse extends RegisterResponse {}

// ===== Password Reset DTOs =====

export interface ForgotPasswordRequest {
  email: string;
}

export interface VerifyCodeRequest {
  email: string;
  code: string;
}

export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
}

// ===== Vote DTO (UC-04) =====

export type VoteType = 'BOOK' | 'MOVIE';

// Голос користувача за твір.
// Endpoint: POST /api/v1/votes
export interface VoteRequest {
  workId: string;
  voteType: VoteType;
}

// Актуальний розподіл голосів за твір.
export interface VoteResponse {
  workId: string;
  totalVotes: number;
  bookVotes: number;
  movieVotes: number;
  bookPercentage: number;
  moviePercentage: number;
}

// ===== Review DTO (UC-05) =====

// Запит на створення відгуку.
// Endpoint: POST /api/v1/reviews
export interface ReviewRequest {
  workId: string;
  text: string;
  isSpoiler: boolean;
  rating: number;
}

// Відповідь зі збереженим відгуком.
export interface ReviewResponse {
  reviewId: string;
  workId: string;
  userId: string;
  text: string;
  isSpoiler: boolean;
  rating: number;
  createdAt: string;
}

// ===== Global Error Schema =====

//форома помилок
export interface ApiError {
  timestamp: string;
  errorCode: string;
  message: string;
  path: string;
  details?: Array<{ field: string; issue: string }>;
}
