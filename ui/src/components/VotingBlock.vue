<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import type { VoteResponse, VoteType } from '../services/types';
import { submitVote, fetchVoteResults } from '../services/votes';
import { useVotesStore } from '../state/votes';
import { useUserStore } from '../state/user';
import { useNotificationsStore } from '../state/notifications';
import { extractErrorMessage } from '../services/error';
import { STR } from '../constants';

const t = STR.detail;

/**
 * SCRUM-70 / SCRUM-71 — блок голосування "Книга vs Фільм".
 *
 * Сценарій:
 *  1) Користувач бачить кнопки "Книга" / "Фільми".
 *  2) Натискає на одну з них → submitVote() → бекенд повертає актуальний
 *     розподіл голосів.
 *  3) Кнопка обраного варіанта стає виділеною; з'являється прогрес-бар
 *     з відсотками (SCRUM-71).
 *  4) Свій голос пам'ятаємо локально (votesStore), щоб після оновлення
 *     сторінки не запитувати повторно і показувати "уже голосували".
 *
 * Якщо користувач не залогінений — показуємо toast-попередження і не
 * відправляємо голос.
 */

const props = defineProps<{
  workId: string;
}>();

const votesStore = useVotesStore();
const userStore = useUserStore();
const notifications = useNotificationsStore();

const result = ref<VoteResponse | null>(null);
const isSubmitting = ref(false);

const myVote = computed<VoteType | null>(() => votesStore.getMyVote(props.workId));

// Відсотки (тільки коли результат завантажений).
const bookPct = computed(() => result.value?.bookPercentage ?? 0);
const moviePct = computed(() => result.value?.moviePercentage ?? 0);
const totalVotes = computed(() => result.value?.totalVotes ?? 0);

// Чи показувати результати: якщо користувач уже голосував — показуємо завжди.
// Інакше — поки нема рішення тримаємо тільки кнопки.
const showResults = computed(() => myVote.value !== null && result.value !== null);

async function loadResults(): Promise<void> {
  try {
    result.value = await fetchVoteResults(props.workId);
  } catch {
    // Тиха помилка — якщо результатів нема, користувач все ще може голосувати.
    result.value = null;
  }
}

async function vote(type: VoteType): Promise<void> {
  if (!userStore.isAuthenticated) {
    notifications.pushWarning(t.voteNeedAuth);
    return;
  }
  if (isSubmitting.value) return;

  isSubmitting.value = true;
  try {
    const response = await submitVote(props.workId, type);
    result.value = response;
    votesStore.setMyVote(props.workId, type);
    notifications.pushSuccess(t.voteThanks);
  } catch (err) {
    notifications.pushError(extractErrorMessage(err));
  } finally {
    isSubmitting.value = false;
  }
}

// Завантажуємо результати при монтуванні і при зміні workId.
onMounted(() => {
  // Якщо користувач уже голосував — підтягуємо актуальні цифри.
  if (votesStore.hasVoted(props.workId)) {
    loadResults();
  }
});

watch(
  () => props.workId,
  () => {
    result.value = null;
    if (votesStore.hasVoted(props.workId)) {
      loadResults();
    }
  }
);
</script>

<template>
  <section class="voting" aria-label="Голосування Книга vs Фільм">
    <h2 class="voting__title">{{ t.voteTitle }}</h2>

    <!-- ── Кнопки ─────────────────────────────────────── -->
    <div class="voting__buttons">
      <button
        type="button"
        class="voting__btn voting__btn--book"
        :class="{
          'voting__btn--active': myVote === 'book',
          'voting__btn--inactive': myVote && myVote !== 'book',
        }"
        :disabled="isSubmitting"
        :aria-pressed="myVote === 'book'"
        @click="vote('book')"
      >
        {{ t.voteBook }}
      </button>

      <span class="voting__vs" aria-hidden="true">{{ t.voteVs }}</span>

      <button
        type="button"
        class="voting__btn voting__btn--film"
        :class="{
          'voting__btn--active': myVote === 'movie',
          'voting__btn--inactive': myVote && myVote !== 'movie',
        }"
        :disabled="isSubmitting"
        :aria-pressed="myVote === 'movie'"
        @click="vote('movie')"
      >
        {{ t.voteFilm }}
      </button>
    </div>

    <!-- ── Результати (SCRUM-71): прогрес-бар з % ────── -->
    <div v-if="showResults" class="voting__results">
      <div class="voting__bar" :aria-label="`${t.voteBook} ${bookPct}%, ${t.voteFilm} ${moviePct}%`">
        <div class="voting__bar-book" :style="{ width: bookPct + '%' }">
          <span v-if="bookPct >= 8" class="voting__bar-pct">{{ bookPct }}%</span>
        </div>
        <div class="voting__bar-film" :style="{ width: moviePct + '%' }">
          <span v-if="moviePct >= 8" class="voting__bar-pct">{{ moviePct }}%</span>
        </div>
      </div>
      <p class="voting__total">{{ t.voteTotal(totalVotes) }}</p>
    </div>

    <p v-else-if="myVote" class="voting__hint">{{ t.voteLoadingResults }}</p>
  </section>
</template>

<style scoped>
.voting {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 18px;
  padding: 24px 16px;
  margin: 0 16px;
  background: var(--color-page);
}

.voting__title {
  margin: 0;
  font-size: 18px;
  font-family: var(--font-display);
  font-weight: 400;
  color: var(--text-on-light);
  text-align: center;
}

/* ── Кнопки ──────────────────────────────────────────── */
.voting__buttons {
  display: inline-flex;
  align-items: center;
  gap: 12px;
}

.voting__btn {
  background: var(--color-card);
  color: var(--text-on-dark);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  padding: 10px 28px;
  font-family: var(--font-display);
  font-size: 16px;
  cursor: pointer;
  box-shadow: var(--shadow-sm);
  transition: all 0.15s;
  min-width: 110px;
}

.voting__btn:hover:not(:disabled) {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.voting__btn:disabled {
  cursor: not-allowed;
  opacity: 0.85;
}

.voting__btn--active {
  background: var(--color-primary);
  border-color: var(--color-primary-dark);
  box-shadow: var(--shadow-md);
}

.voting__btn--inactive {
  background: var(--color-card);
  opacity: 0.6;
}

.voting__vs {
  font-family: var(--font-display);
  font-size: 18px;
  font-weight: 700;
  color: var(--color-primary);
}

/* ── Прогрес-бар результатів (SCRUM-71) ──────────────── */
.voting__results {
  width: 100%;
  max-width: 500px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.voting__bar {
  display: flex;
  width: 100%;
  height: 28px;
  border-radius: var(--radius-pill);
  overflow: hidden;
  background: var(--color-panel-box);
  border: 1px solid var(--color-card);
}

.voting__bar-book,
.voting__bar-film {
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-display);
  font-size: 12px;
  font-weight: 600;
  color: var(--text-on-primary);
  transition: width 0.4s ease;
  min-width: 0;
}

.voting__bar-book {
  background: var(--color-primary);
}

.voting__bar-film {
  background: var(--color-card);
}

.voting__bar-pct {
  white-space: nowrap;
}

.voting__total {
  margin: 0;
  text-align: center;
  font-family: var(--font-body);
  font-size: 13px;
  color: var(--text-muted);
}

.voting__hint {
  margin: 0;
  font-size: 13px;
  color: var(--text-muted);
  font-family: var(--font-body);
}

@media (max-width: 480px) {
  .voting__buttons {
    flex-direction: column;
    gap: 8px;
  }
  .voting__btn {
    min-width: 180px;
  }
}
</style>
