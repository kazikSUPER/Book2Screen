<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { DifferencePoint } from '../services/types';
import { STR } from '../constants';

const t = STR.detail;

/**
 * SCRUM-68 — Інтерактивна карта відмінностей між книгою і екранізацією.
 * Killer-фіча проєкту: користувач бачить горизонтальну вісь точок 1..N,
 * клікає на точку — знизу з'являється деталь (заголовок + дві колонки).
 *
 * Спойлери — текст блюриться, поки користувач не клікне "показати".
 */

const props = defineProps<{
  points: DifferencePoint[];
}>();

// За замовчуванням — обрана перша точка.
const activeIdx = ref(0);

// Якщо змінили твір (props.points повністю інший масив) — обнуляємо вибір.
watch(
  () => props.points,
  () => {
    activeIdx.value = 0;
  }
);

const activePoint = computed<DifferencePoint | null>(() => {
  return props.points[activeIdx.value] ?? null;
});

// Локально пам'ятаємо, які спойлери користувач вже розкрив.
// Скидається при зміні твору (по ключу activePoint.id).
const revealedSpoilers = ref<Record<string, boolean>>({});

const isRevealed = computed(() => {
  if (!activePoint.value) return true;
  if (!activePoint.value.isSpoiler) return true;
  return revealedSpoilers.value[activePoint.value.id] === true;
});

function reveal(): void {
  if (activePoint.value) revealedSpoilers.value[activePoint.value.id] = true;
}

function setActive(i: number): void {
  activeIdx.value = i;
}
</script>

<template>
  <section v-if="points.length > 0" class="diff-map">
    <h2 class="diff-map__title">{{ t.differencesTitle }}</h2>

    <!-- ── Горизонтальна вісь точок ─────────────────────── -->
    <div class="diff-map__axis" role="tablist">
      <div class="diff-map__line" aria-hidden="true"></div>
      <button
        v-for="(p, i) in points"
        :key="p.id"
        role="tab"
        :aria-selected="i === activeIdx"
        :aria-label="`Точка ${i + 1}: ${p.title}`"
        type="button"
        class="diff-map__point"
        :class="{ 'diff-map__point--active': i === activeIdx }"
        @click="setActive(i)"
      >
        {{ i + 1 }}
      </button>
    </div>

    <!-- ── Деталі обраної точки ─────────────────────────── -->
    <article v-if="activePoint" class="diff-map__detail">
      <header class="diff-map__detail-head">
        <h3 class="diff-map__detail-title">{{ activePoint.title }}</h3>
      </header>

      <div class="diff-map__columns" :class="{ 'diff-map__columns--blurred': !isRevealed }">
        <div class="diff-map__col">
          <div class="diff-map__col-label">{{ t.voteBook }}</div>
          <p class="diff-map__col-text">{{ activePoint.bookText }}</p>
        </div>
        <div class="diff-map__col">
          <div class="diff-map__col-label">{{ t.adapted }}</div>
          <p class="diff-map__col-text">{{ activePoint.filmText }}</p>
        </div>
      </div>

      <div v-if="!isRevealed" class="diff-map__spoiler-overlay">
        <p class="diff-map__spoiler-msg">{{ t.spoilerWarning }}</p>
        <button class="diff-map__reveal" type="button" @click="reveal">{{ t.spoilerReveal }}</button>
      </div>
    </article>
  </section>
</template>

<style scoped>
.diff-map {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 24px 16px;
  background: var(--color-page);
}

.diff-map__title {
  margin: 0;
  font-size: 24px;
  font-family: var(--font-display);
  font-weight: 400;
  color: var(--text-on-light);
  text-align: center;
}

/* ── Горизонтальна вісь ──────────────────────────────────── */
.diff-map__axis {
  position: relative;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 12px;
  max-width: 700px;
  width: 100%;
  margin: 0 auto;
}

.diff-map__line {
  position: absolute;
  left: 24px;
  right: 24px;
  top: 50%;
  height: 2px;
  background: var(--color-card);
  transform: translateY(-50%);
  z-index: 0;
}

.diff-map__point {
  position: relative;
  z-index: 1;
  width: 36px;
  height: 36px;
  border-radius: var(--radius-pill);
  border: 2px solid var(--color-card);
  background: var(--color-card);
  color: var(--text-on-dark);
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
}

.diff-map__point:hover {
  transform: scale(1.1);
}

.diff-map__point--active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: var(--text-on-primary);
  transform: scale(1.15);
  box-shadow: var(--shadow-sm);
}

/* ── Картка деталі ──────────────────────────────────────── */
.diff-map__detail {
  position: relative;
  background: var(--color-card);
  border-radius: var(--radius-md);
  padding: 20px;
  box-shadow: var(--shadow-md);
  max-width: 800px;
  width: 100%;
  margin: 0 auto;
}

.diff-map__detail-head {
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.diff-map__detail-title {
  font-size: 18px;
  font-family: var(--font-display);
  color: var(--text-on-dark);
  text-align: center;
  margin: 0;
  font-weight: 400;
}

.diff-map__columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  transition: filter 0.2s;
}

.diff-map__columns--blurred {
  filter: blur(6px);
  pointer-events: none;
  user-select: none;
}

.diff-map__col {
  background: var(--color-panel-box);
  border-radius: var(--radius-sm);
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.diff-map__col-label {
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 600;
  color: var(--text-on-light);
}

.diff-map__col-text {
  font-family: var(--font-body);
  font-size: 14px;
  line-height: 1.5;
  color: var(--text-on-light);
  margin: 0;
}

/* ── Спойлер-оверлей ──────────────────────────────────────── */
.diff-map__spoiler-overlay {
  position: absolute;
  inset: 50px 20px 20px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  background: rgba(61, 15, 26, 0.7);
  border-radius: var(--radius-sm);
  pointer-events: auto;
}

.diff-map__spoiler-msg {
  margin: 0;
  font-family: var(--font-display);
  color: var(--text-on-dark);
  font-size: 14px;
}

.diff-map__reveal {
  background: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-md);
  padding: 8px 24px;
  font-family: var(--font-display);
  font-size: 14px;
  cursor: pointer;
  box-shadow: var(--shadow-sm);
}

.diff-map__reveal:hover {
  background: var(--color-primary-hover);
}

@media (max-width: 640px) {
  .diff-map__columns {
    grid-template-columns: 1fr;
  }
  .diff-map__point {
    width: 28px;
    height: 28px;
    font-size: 12px;
  }
}
</style>
