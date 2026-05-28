<script lang="ts">
// Експортовані типи для зовнішніх споживачів — мають бути в окремому
// (не-setup) блоці, оскільки <script setup> не дозволяє export.
export type FilterSection = 'sort' | 'genres' | 'countries' | 'years' | 'rating';
</script>

<script setup lang="ts">
import { computed } from 'vue';
import { useFiltersStore } from '../state/filters';
import { GENRES, COUNTRIES, SORT_OPTIONS_VERTICAL, MIN_YEAR, MAX_YEAR } from '../constants';

/**
 * Універсальна вертикальна панель фільтрів. Використовується на Home (компактно)
 * і Search (повний набір). Всі поля прив'язані до useFiltersStore — це означає,
 * що з будь-якого місця можна програмно поставити фільтр і він тут одразу
 * відобразиться.
 */

const props = withDefaults(
  defineProps<{
    mobileOpen?: boolean;
    sections?: FilterSection[];
  }>(),
  {
    mobileOpen: false,
    sections: () => ['sort', 'genres', 'countries', 'years', 'rating'],
  }
);

defineEmits<{
  closeMobile: [];
}>();

const filters = useFiltersStore();

function show(section: FilterSection): boolean {
  return props.sections.includes(section);
}

const genres = GENRES;
const countries = COUNTRIES;
const sortOptions = SORT_OPTIONS_VERTICAL;

const yearMinInput = computed({
  get: () => filters.yearMin ?? '',
  set: (v: number | string) => {
    const n = Number(v);
    filters.yearMin = v === '' || isNaN(n) ? null : Math.min(Math.max(n, MIN_YEAR), MAX_YEAR);
  },
});

const yearMaxInput = computed({
  get: () => filters.yearMax ?? '',
  set: (v: number | string) => {
    const n = Number(v);
    filters.yearMax = v === '' || isNaN(n) ? null : Math.min(Math.max(n, MIN_YEAR), MAX_YEAR);
  },
});

const minRatingInput = computed({
  get: () => filters.minRating ?? 0,
  set: (v: number) => {
    filters.minRating = v === 0 ? null : v;
  },
});

function toggleGenre(g: string): void {
  filters.genre = filters.genre === g ? null : g;
}

function toggleCountry(c: string): void {
  filters.country = filters.country === c ? null : c;
}
</script>

<template>
  <aside class="filter-panel" :class="{ 'mobile-open': mobileOpen }">
    <button v-if="mobileOpen" class="mobile-close" aria-label="Закрити фільтри" @click="$emit('closeMobile')">✕</button>

    <div v-if="show('sort')" class="filter-box">
      <h3 class="filter-title">Сортування</h3>
      <select v-model="filters.sortBy" class="filter-select">
        <option v-for="opt in sortOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
    </div>

    <div v-if="show('genres')" class="filter-box">
      <h3 class="filter-title">Жанри</h3>
      <ul class="filter-list">
        <li
          v-for="g in genres"
          :key="g"
          class="filter-item"
          :class="{ active: filters.genre === g }"
          @click="toggleGenre(g)"
        >
          {{ g }}
        </li>
      </ul>
    </div>

    <div v-if="show('countries')" class="filter-box">
      <h3 class="filter-title">Країна</h3>
      <ul class="filter-list">
        <li
          v-for="c in countries"
          :key="c"
          class="filter-item"
          :class="{ active: filters.country === c }"
          @click="toggleCountry(c)"
        >
          {{ c }}
        </li>
      </ul>
    </div>

    <div v-if="show('years')" class="filter-box">
      <h3 class="filter-title">Рік</h3>
      <div class="year-range">
        <input
          v-model.number="yearMinInput"
          type="number"
          class="year-input"
          :placeholder="String(MIN_YEAR)"
          :min="MIN_YEAR"
          :max="MAX_YEAR"
        />
        <span class="year-dash">—</span>
        <input
          v-model.number="yearMaxInput"
          type="number"
          class="year-input"
          :placeholder="String(MAX_YEAR)"
          :min="MIN_YEAR"
          :max="MAX_YEAR"
        />
      </div>
    </div>

    <div v-if="show('rating')" class="filter-box">
      <h3 class="filter-title">
        Мін. рейтинг: <span class="rating-value">{{ minRatingInput }}</span>
      </h3>
      <input v-model.number="minRatingInput" type="range" min="0" max="10" step="0.5" class="rating-slider" />
    </div>

    <button v-if="filters.hasActiveFilters" class="clear-btn" @click="filters.clearAll()">Очистити всі фільтри</button>
  </aside>
</template>

<style scoped>
.filter-panel {
  width: var(--filter-panel-width);
  flex-shrink: 0;
  background-color: var(--color-panel);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-sm);
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 18px;
  overflow-y: auto;
  max-height: calc(100vh - 100px);
}

@media (max-width: 768px) {
  .filter-panel {
    position: fixed;
    inset: 0 auto 0 0;
    width: 280px;
    max-width: 85vw;
    z-index: 200;
    transform: translateX(-100%);
    transition: transform 0.25s ease;
    box-shadow: 4px 0 12px rgba(0, 0, 0, 0.15);
    max-height: 100vh;
  }
  .filter-panel.mobile-open {
    transform: translateX(0);
  }
}

.mobile-close {
  display: none;
  align-self: flex-end;
  background: none;
  border: none;
  font-size: 22px;
  cursor: pointer;
  color: var(--text-on-light);
  padding: 4px 8px;
}

@media (max-width: 768px) {
  .mobile-close {
    display: block;
  }
}

.filter-box {
  background-color: var(--color-panel-box);
  border: 1px solid var(--border-default);
  border-radius: var(--radius-md);
  padding: 8px 14px;
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-title {
  font-size: 22px;
  font-family: var(--font-display);
  color: var(--text-on-light);
  margin: 0;
  font-weight: 400;
}

.filter-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
}

.filter-item {
  font-size: 18px;
  font-family: var(--font-display);
  color: var(--text-on-light);
  cursor: pointer;
  padding: 2px 4px;
  border-radius: var(--radius-xs);
  transition:
    background 0.15s,
    color 0.15s;
}

.filter-item:hover {
  background: var(--color-primary);
  color: var(--text-on-dark);
}

.filter-item.active {
  background: var(--color-primary);
  color: var(--text-on-dark);
  font-weight: 600;
}

.filter-select {
  padding: 8px 10px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--color-input-bg);
  font-family: var(--font-display);
  font-size: 16px;
  color: var(--text-on-light);
  cursor: pointer;
  outline: none;
}

.filter-select:focus {
  border-color: var(--color-primary);
}

.year-range {
  display: flex;
  align-items: center;
  gap: 8px;
}

.year-input {
  width: 100%;
  padding: 6px 8px;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-xs);
  background: var(--color-input-bg);
  font-family: var(--font-display);
  font-size: 14px;
  color: var(--text-on-light);
  outline: none;
}

.year-input:focus {
  border-color: var(--color-primary);
}

.year-dash {
  color: var(--text-on-light);
  font-weight: 700;
}

.rating-slider {
  width: 100%;
  cursor: pointer;
  accent-color: var(--color-primary);
}

.rating-value {
  color: var(--color-primary);
  font-weight: 700;
}

.clear-btn {
  margin-top: 4px;
  padding: 10px;
  background: var(--color-header);
  color: var(--text-on-dark);
  border: none;
  border-radius: var(--radius-sm);
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  transition: background 0.2s;
}

.clear-btn:hover {
  background: var(--color-primary);
}
</style>
