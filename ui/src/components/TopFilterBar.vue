<script setup lang="ts">
import { computed } from 'vue';
import { useFiltersStore } from '../state/filters';
import { GENRES, SORT_OPTIONS_TOP, STR } from '../constants';

/**
 * Горизонтальна панель фільтрів для TopView (SCRUM-67).
 * За дизайном Figma: рожевий блок із трьома dropdown'ами і чекбоксом справа.
 *
 * Прив'язується до глобального useFiltersStore — поведінка узгоджена
 * з вертикальною FilterPanel на Home/Search.
 */

const filters = useFiltersStore();

// "Рік виходу" у TopView — це один обраний рік. Технічно ми тримаємо
// діапазон [yearMin, yearMax], тому коли користувач обирає рік X, ставимо
// yearMin = yearMax = X. Коли обирає "Будь-який" — обидва null.
const selectedYear = computed<number | null>({
  get() {
    if (filters.yearMin !== null && filters.yearMin === filters.yearMax) {
      return filters.yearMin;
    }
    return null;
  },
  set(year) {
    if (year === null) {
      filters.yearMin = null;
      filters.yearMax = null;
    } else {
      filters.yearMin = year;
      filters.yearMax = year;
    }
  },
});

// Список років від поточного до 1950 (ширше — рідко потрібно).
const CURRENT_YEAR = new Date().getFullYear();
const yearOptions: number[] = Array.from({ length: CURRENT_YEAR - 1949 }, (_, i) => CURRENT_YEAR - i);

const genres = GENRES;
const sortOptions = SORT_OPTIONS_TOP;
const t = STR.top;
</script>

<template>
  <div class="top-filter-bar">
    <div class="top-filter-bar__group">
      <!-- Рік виходу -->
      <label class="top-filter-bar__field">
        <select v-model.number="selectedYear" class="top-filter-bar__select">
          <option :value="null">{{ t.yearOption }}</option>
          <option v-for="y in yearOptions" :key="y" :value="y">{{ y }}</option>
        </select>
      </label>

      <!-- Жанр -->
      <label class="top-filter-bar__field">
        <select v-model="filters.genre" class="top-filter-bar__select">
          <option :value="null">{{ t.genreOption }}</option>
          <option v-for="g in genres" :key="g" :value="g">{{ g }}</option>
        </select>
      </label>

      <!-- Сортування -->
      <label class="top-filter-bar__field">
        <select v-model="filters.sortBy" class="top-filter-bar__select">
          <option v-for="opt in sortOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
        </select>
      </label>
    </div>

    <!-- Чекбокс "Лише з картою відмінностей" -->
    <label class="top-filter-bar__checkbox">
      <input v-model="filters.onlyWithMap" type="checkbox" />
      <span class="top-filter-bar__checkmark" aria-hidden="true">
        <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
          <path
            d="M2 7.5L5.5 11L12 3.5"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
        </svg>
      </span>
      <span class="top-filter-bar__label">{{ t.onlyWithMap }}</span>
    </label>
  </div>
</template>

<style scoped>
.top-filter-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px 20px;
  background-color: var(--color-panel-box);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  flex-wrap: wrap;
}

.top-filter-bar__group {
  display: flex;
  gap: 12px;
  flex: 1;
  flex-wrap: wrap;
  min-width: 0;
}

.top-filter-bar__field {
  flex: 1;
  min-width: 160px;
  max-width: 240px;
}

.top-filter-bar__select {
  width: 100%;
  padding: 9px 12px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  font-family: var(--font-display);
  font-size: 14px;
  color: var(--text-on-light);
  cursor: pointer;
  outline: none;
  appearance: none;
  /* стрілочка справа */
  background-image:
    linear-gradient(45deg, transparent 50%, var(--text-on-light) 50%),
    linear-gradient(135deg, var(--text-on-light) 50%, transparent 50%);
  background-position:
    calc(100% - 14px) 50%,
    calc(100% - 9px) 50%;
  background-size:
    5px 5px,
    5px 5px;
  background-repeat: no-repeat;
  padding-right: 28px;
}

.top-filter-bar__select:focus {
  border-color: var(--color-primary);
}

/* ── Кастомний чекбокс ───────────────────────────────────── */
.top-filter-bar__checkbox {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-family: var(--font-display);
  font-size: 14px;
  color: var(--text-on-light);
  user-select: none;
  flex-shrink: 0;
}

.top-filter-bar__checkbox input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.top-filter-bar__checkmark {
  width: 22px;
  height: 22px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-xs);
  background: var(--color-input-bg);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: transparent;
  transition: all 0.15s;
}

.top-filter-bar__checkbox input:checked ~ .top-filter-bar__checkmark {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: var(--text-on-primary);
}

.top-filter-bar__checkbox input:focus-visible ~ .top-filter-bar__checkmark {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

@media (max-width: 768px) {
  .top-filter-bar {
    flex-direction: column;
    align-items: stretch;
  }
  .top-filter-bar__field {
    max-width: none;
  }
}
</style>
