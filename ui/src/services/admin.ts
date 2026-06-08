import { apiClient } from './api';
import type { AdaptationDto, BookScreenItem, ReportResponse, DifferencePoint } from './types';
import { fetchWorks, fetchWorkById } from './works';
import { ALL_ITEMS } from './items';
import { USE_MOCK_FALLBACK } from './env';

function toAdaptationDto(item: Partial<BookScreenItem> & { id?: string; type?: string }): AdaptationDto {
  return {
    id: item.id,
    title: item.title ?? '',
    type: item.type ?? 'movie',
    description: item.description,
    releaseYear: item.filmYear ?? item.year,
    posterUrl: item.filmPoster ?? item.poster,
    country: item.filmCountry ?? item.country,
    studio: item.director,
  };
}

function fromAdaptationDto(a: AdaptationDto): BookScreenItem {
  return {
    id: a.id ?? '',
    title: a.title,
    year: a.releaseYear ?? 0,
    genre: '',
    country: a.country ?? '',
    poster: a.posterUrl ?? '',
    bookRating: 0,
    filmRating: 0,
    description: a.description ?? '',
    director: a.studio,
    filmYear: a.releaseYear,
    filmPoster: a.posterUrl,
    filmCountry: a.country,
  };
}

export async function fetchAllBooks(): Promise<BookScreenItem[]> {
  return fetchWorks();
}

export async function fetchAdminBook(id: string): Promise<BookScreenItem> {
  return fetchWorkById(id);
}

export async function createBook(book: Omit<BookScreenItem, 'id'> & { type?: string }): Promise<BookScreenItem> {
  const dto = toAdaptationDto(book);
  try {
    const response = await apiClient.post<AdaptationDto>('/api/v1/admin/adaptations', dto);
    return fromAdaptationDto(response.data);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      const fake: BookScreenItem = {
        ...(book as BookScreenItem),
        id: `local-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      };
      console.warn('[admin] Mock create:', fake.title);
      return fake;
    }
    throw err;
  }
}

export async function updateBook(
  id: string,
  patch: Partial<Omit<BookScreenItem, 'id'>> & { type?: string }
): Promise<BookScreenItem> {
  const dto = toAdaptationDto({ ...patch, id });
  try {
    const response = await apiClient.put<AdaptationDto>(`/api/v1/admin/adaptations/${id}`, dto);
    return fromAdaptationDto(response.data);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[admin] Mock update:', id);
      const local = ALL_ITEMS.find((b) => b.id === id);
      return { ...(local ?? ({} as BookScreenItem)), ...patch, id } as BookScreenItem;
    }
    throw err;
  }
}

export async function deleteBook(id: string): Promise<void> {
  try {
    await apiClient.delete(`/api/v1/admin/adaptations/${id}`);
  } catch (err) {
    if (!USE_MOCK_FALLBACK) throw err;
    console.warn('[admin] Mock delete:', id);
  }
}

export type ReportStatus = 'pending' | 'approved' | 'rejected' | 'marked-spoiler';

export interface ReportedComment {
  reportId: string;
  reason: string;
  status: ReportStatus;
  createdAt: string;
  review: {
    reviewId: string;
    workId: string;
    userId: string;
    text: string;
    isSpoiler: boolean;
    rating: number;
    createdAt: string;
  };
}

function normalizeStatus(raw: string | undefined): ReportStatus {
  const s = (raw ?? '').toLowerCase().trim();
  if (s === 'approved' || s === 'rejected' || s === 'marked-spoiler') return s;
  if (s === 'spoiler' || s === 'marked_spoiler' || s === 'markedspoiler') return 'marked-spoiler';
  return 'pending';
}

function mapReport(r: ReportResponse): ReportedComment {
  return {
    reportId: r.reportId,
    reason: r.reason ?? '',
    status: normalizeStatus(r.status),
    createdAt: r.createdAt,
    review: {
      reviewId: r.reviewId,
      workId: '',
      userId: r.userId,
      text: r.reviewText ?? '',
      isSpoiler: false,
      rating: 0,
      createdAt: r.createdAt,
    },
  };
}

export async function fetchReports(): Promise<ReportedComment[]> {
  try {
    const response = await apiClient.get<ReportResponse[]>('/api/v1/admin/reports');
    return response.data.map(mapReport);
  } catch (err) {
    if (USE_MOCK_FALLBACK) {
      console.warn('[admin] Backend unavailable, mock reports', err);
      return seedReports();
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
}

function seedReports(): ReportedComment[] {
  const hpId = '11111111-1111-1111-1111-111111111111';
  return Array.from({ length: 10 }, (_, i) => ({
    reportId: `rep-${i + 1}`,
    reason: 'Спойлер',
    status: 'pending' as ReportStatus,
    createdAt: new Date(Date.now() - i * 3600_000).toISOString(),
    review: {
      reviewId: `rev-${i + 1}`,
      workId: hpId,
      userId: `u-${i + 100}`,
      text: 'В кінці книги головні герої…',
      isSpoiler: false,
      rating: 4,
      createdAt: new Date(Date.now() - i * 3600_000).toISOString(),
    },
  }));
}

export type { DifferencePoint };
