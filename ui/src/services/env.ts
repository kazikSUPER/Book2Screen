/**
 * Централізовані прапори середовища.
 *
 * Раніше у кожному сервісі (works, admin, reviews, votes, profile) була
 * локальна константа `USE_MOCK_FALLBACK = true`. Це порушувало Zero-Mocks
 * Policy (Етап 2, Крок 2) — і взагалі не давало вимкнути моки на проді
 * без правки коду. Тепер один прапор — з env, дефолт `false`.
 *
 * У dev-режимі (npm run dev) розробник може поставити
 *   VITE_USE_MOCK_FALLBACK=true
 * у .env.local — і працювати без бекенду. На проді — лишається `false`.
 */

function readBool(value: string | undefined, fallback: boolean): boolean {
  if (value === undefined) return fallback;
  return value === 'true' || value === '1';
}

// У dev-режимі дефолт — true (щоб новий розробник без бекенду одразу побачив UI).
// На проді — false (жодних моків).
const DEFAULT_USE_MOCK = import.meta.env.DEV;

export const USE_MOCK_FALLBACK: boolean = readBool(import.meta.env.VITE_USE_MOCK_FALLBACK, DEFAULT_USE_MOCK);

export const API_URL: string = import.meta.env.VITE_API_URL || '';
