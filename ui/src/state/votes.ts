import { defineStore } from 'pinia';
import { computed } from 'vue';
import { usePersistedRef } from '../composables/usePersistedRef';
import type { VoteType } from '../services/types';

/**
 * SCRUM-70 — пам'ятаємо ЗА ЩО проголосував користувач, щоб не давати
 * проголосувати ще раз і показувати йому виділеним вибраний варіант.
 *
 * Persist у localStorage: переживає перезавантаження сторінки (вимога Етапу 4).
 *
 * Це окрема структура від реальних агрегатів з бекенду — там зберігається
 * лічильник голосів усіх користувачів. Тут — тільки ВЛАСНИЙ голос юзера.
 */

interface UserVote {
  workId: string;
  voteType: VoteType;
}

export const useVotesStore = defineStore('votes', () => {
  const myVotes = usePersistedRef<UserVote[]>('b2s_my_votes', []);

  function getMyVote(workId: string): VoteType | null {
    return myVotes.value.find((v) => v.workId === workId)?.voteType ?? null;
  }

  function setMyVote(workId: string, type: VoteType): void {
    const idx = myVotes.value.findIndex((v) => v.workId === workId);
    if (idx >= 0) {
      myVotes.value[idx].voteType = type;
    } else {
      myVotes.value.push({ workId, voteType: type });
    }
  }

  function hasVoted(workId: string): boolean {
    return getMyVote(workId) !== null;
  }

  const totalVoted = computed(() => myVotes.value.length);

  return { myVotes, getMyVote, setMyVote, hasVoted, totalVoted };
});
