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

// Стабільний ключ: id з беку якщо є (Guid?), інакше — індекс точки.
const activeKey = computed(() => activePoint.value?.id ?? `idx-${activeIdx.value}`);

const isRevealed = computed(() => {
  if (!activePoint.value) return true;
  if (!activePoint.value.isSpoiler) return true;
  return revealedSpoilers.value[activeKey.value] === true;
});

function reveal(): void {
  if (activePoint.value) revealedSpoilers.value[activeKey.value] = true;
}

function setActive(i: number): void {
  activeIdx.value = i;
}
</script>

<template>
  <section class="diff-map">
    <h2 class="diff-map__title">{{ t.differencesTitle }}</h2>

    <!-- Плейсхолдер коли точок ще немає (адмін їх ще не додав). -->
    <p v-if="points.length === 0" class="diff-map__empty">Карта відмінностей для цього твору ще не створена</p>

    <!-- ── Горизонтальна вісь точок ─────────────────────── -->
    <div v-else class="diff-map__axis" role="tablist">
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
          <div class="diff-map__col-label">Книга</div>
          <p class="diff-map__col-text">{{ activePoint.bookText }}</p>
        </div>
        <div class="diff-map__col">
          <div class="diff-map__col-label">Екранізація</div>
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
  background: transparent;
}

.diff-map__title {
  margin: 0 0 16px 0;
  font-size: 24px;
  font-family: var(--font-display);
  font-weight: 500;
  color: #23080a;
  text-align: center;
}

.diff-map__empty {
  margin: 0;
  text-align: center;
  font-family: var(--font-body);
  font-size: 14px;
  color: var(--text-muted);
  padding: 24px;
  background: var(--color-panel-box);
  border-radius: var(--radius-sm);
  max-width: 600px;
  width: 100%;
  align-self: center;
}

/* ── Горизонтальна вісь ──────────────────────────────────── */
.diff-map__axis {
  position: relative;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 40px 24px 24px 24px;
  max-width: 600px;
  width: 100%;
  margin: 0 auto;
}

.diff-map__line {
  position: absolute;
  left: 36px;
  right: 36px;
  top: 55%;
  height: 1px;
  background: #333;
  transform: translateY(-50%);
  z-index: 0;
}

/* Засічки, що проходять через першу і останню точки */
.diff-map__line::before,
.diff-map__line::after {
  content: '';
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 1px;
  height: 80px;
  background: #333;
}
.diff-map__line::before {
  left: 0;
}
.diff-map__line::after {
  right: 0;
}

.diff-map__point {
  position: relative;
  z-index: 1;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  background: #23080a; /* Темні кружечки */
  color: #fff;
  font-family: var(--font-display);
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.diff-map__point::after {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 48px;
  height: 48px;
  pointer-events: auto;
}

.diff-map__point:hover {
  transform: scale(1.1);
}

.diff-map__point--active {
  background: #5a141c; /* Трохи світліший при виборі */
  transform: scale(1.15);
}

/* ── Картка деталі ──────────────────────────────────────── */
.diff-map__detail {
  position: relative;
  background-color: #b18a88; /* Світло-коричневий фон картки (як на скріні) */
  border-radius: var(--radius-md);
  padding: 24px;
  max-width: 800px;
  width: 100%;
  margin: 0 auto;
}

.diff-map__detail-head {
  margin-bottom: 24px;
}

.diff-map__detail-title {
  font-size: 20px;
  font-family: var(--font-display);
  color: #23080a;
  text-align: center;
  margin: 0;
  font-weight: 500;
}

.diff-map__columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  transition: filter 0.2s;
}

.diff-map__columns--blurred {
  filter: blur(6px);
  pointer-events: none;
  user-select: none;
}

.diff-map__col {
  background-color: #391418; /* Темно-бордовий фон колонок */
  border-radius: var(--radius-sm);
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.diff-map__col-label {
  font-family: var(--font-display);
  font-size: 16px;
  font-weight: 500;
  color: #fff;
  text-align: center;
  margin-bottom: 8px;
}

.diff-map__col-text {
  font-family: var(--font-body);
  font-size: 15px;
  line-height: 1.5;
  color: #fff;
  margin: 0;
  text-align: center;
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
