import { apiClient } from './api';
import type { BookScreenItem, DifferencePoint, ReviewResponse } from './types';
import { ALL_ITEMS } from './items';
import { USE_MOCK_FALLBACK } from './env';

/**
 * SCRUM-143 — Admin Panel API.
 *
 * Backend (заплановані ендпоінти):
 *   POST   /api/v1/works                 — створити твір
 *   PUT    /api/v1/works/:id             — оновити твір
 *   DELETE /api/v1/works/:id             — видалити твір
 *   GET    /api/v1/admin/reports         — список скарг на коментарі
 *   POST   /api/v1/admin/reports/:id/approve   — схвалити (видалити коментар)
 *   POST   /api/v1/admin/reports/:id/reject    — відхилити скаргу (залишити коментар)
 *   POST   /api/v1/admin/reports/:id/spoiler   — позначити коментар як спойлер
 *
 * Mock-fallback: працюємо з in-memory копією ALL_ITEMS, щоб дії в UI були
 * видимі впродовж сесії. Скидається при перезавантаженні.
 */

const localBooks: BookScreenItem[] = [...ALL_ITEMS];

export type ReportStatus = 'pending' | 'approved' | 'rejected' | 'marked-spoiler';

export interface ReportedComment {
  reportId: string;
  review: ReviewResponse;
  reason: string;
  status: ReportStatus;
  createdAt: string;
}

const localReports: ReportedComment[] = seedReports();

// ── Books CRUD ──────────────────────────────────────────────

export async function fetchAllBooks(): Promise<BookScreenItem[]> {
  try {
    const response = await apiClient.get<BookScreenItem[]>('/api/v1/works');
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[admin] Backend unavailable, returning local list', err);
      return [...localBooks];
    }
    throw err;
  }
}

export async function createBook(book: Omit<BookScreenItem, 'id'>): Promise<BookScreenItem> {
  const fake: BookScreenItem = {
    ...book,
    id: `local-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
  };
  try {
    const response = await apiClient.post<BookScreenItem>('/api/v1/works', book);
    localBooks.push(response.data);
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[admin] Mock create:', fake);
      localBooks.push(fake);
      return fake;
    }
    throw err;
  }
}

export async function updateBook(id: string, patch: Partial<Omit<BookScreenItem, 'id'>>): Promise<BookScreenItem> {
  try {
    const response = await apiClient.put<BookScreenItem>(`/api/v1/works/${id}`, patch);
    const idx = localBooks.findIndex((b) => b.id === id);
    if (idx >= 0) localBooks[idx] = response.data;
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[admin] Mock update:', { id, patch });
      const idx = localBooks.findIndex((b) => b.id === id);
      if (idx >= 0) {
        localBooks[idx] = { ...localBooks[idx], ...patch };
        return localBooks[idx];
      }
      throw new Error('Книгу не знайдено', { cause: err });
    }
    throw err;
  }
}

export async function deleteBook(id: string): Promise<void> {
  try {
    await apiClient.delete(`/api/v1/works/${id}`);
  } catch (err) {
    if (!USE_MOCK_FALLBACK) throw err;
    console.warn('[admin] Mock delete:', id);
  }
  const idx = localBooks.findIndex((b) => b.id === id);
  if (idx >= 0) localBooks.splice(idx, 1);
}

// ── Reports / Comment moderation ───────────────────────────

export async function fetchReports(): Promise<ReportedComment[]> {
  try {
    const response = await apiClient.get<ReportedComment[]>('/api/v1/admin/reports');
    return response.data;
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[admin] Backend unavailable, returning mock reports', err);
      return [...localReports];
    }
    throw err;
  }
}

export async function moderateReport(reportId: string, action: 'approve' | 'reject' | 'spoiler'): Promise<void> {
  try {
    await apiClient.post(`/api/v1/admin/reports/${reportId}/${action}`);
  } catch (err) {
    if (!USE_MOCK_FALLBACK) throw err;
    console.warn('[admin] Mock moderate:', { reportId, action });
  }
  const r = localReports.find((x) => x.reportId === reportId);
  if (r) {
    r.status = action === 'approve' ? 'approved' : action === 'reject' ? 'rejected' : 'marked-spoiler';
  }
}

// ── Mock seed ──────────────────────────────────────────────

function seedReports(): ReportedComment[] {
  const hpId = '11111111-1111-1111-1111-111111111111';
  const sample: ReportedComment[] = Array.from({ length: 10 }, (_, i) => ({
    reportId: `rep-${i + 1}`,
    reason: 'Спойлер',
    status: 'pending',
    createdAt: new Date(Date.now() - i * 3600_000).toISOString(),
    review: {
      reviewId: `rev-${i + 1}`,
      workId: hpId,
      userId: `u-${i + 100}`,
      userNickname: 'Користувач',
      text: 'В кінці книги головні герої…',
      isSpoiler: false,
      rating: 4,
      createdAt: new Date(Date.now() - i * 3600_000).toISOString(),
    },
  }));
  return sample;
}

// Експорт типу для UI компонента (DifferencePoint редагування)
export type { DifferencePoint };
