import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';

/**
 * SCRUM-68 — особиста "зіркова" оцінка користувача (1..5) для кожного твору.
 * Окремо для книги і для екранізації.
 *
 * Persist у localStorage + sync на бекенд через POST /api/v1/ratings
 * (services/ratings.ts, fire-and-forget — UI не чекає на відповідь).
 */

export type RatingTarget = 'book' | 'film';

interface UserRating {
  workId: string;
  target: RatingTarget;
  value: number; // 1..5
}

import { submitRating } from '../services/ratings';

export const useUserRatingsStore = defineStore('user-ratings', () => {
  const ratings = usePersistedRef<UserRating[]>('b2s_user_ratings', []);

  function getRating(workId: string, target: RatingTarget): number {
    const entry = ratings.value.find((r) => r.workId === workId && r.target === target);
    return entry?.value ?? 0;
  }

  function setRating(workId: string, target: RatingTarget, value: number): void {
    const idx = ratings.value.findIndex((r) => r.workId === workId && r.target === target);
    const safe = Math.max(0, Math.min(5, Math.round(value)));
    if (safe === 0) {
      if (idx >= 0) ratings.value.splice(idx, 1);
    } else {
      if (idx >= 0) {
        ratings.value[idx].value = safe;
      } else {
        ratings.value.push({ workId, target, value: safe });
      }
    }

    // Fire-and-forget sync to the backend
    submitRating({ workId, target, value: safe }).catch((err) => {
      console.error('[ratings] Failed to sync rating to backend:', err);
    });
  }

  // Для ProfileView (SCRUM-64) — секція "Мої оцінки".
  const ratedWorkIds = computed(() => {
    const ids = new Set(ratings.value.map((r) => r.workId));
    return Array.from(ids);
  });

  return { ratings, getRating, setRating, ratedWorkIds };
});
