<script setup lang="ts">
import { ref, computed } from 'vue';

/**
 * Зірковий рейтинг (5 зірок).
 *
 * Режими:
 * - readonly (за замовчуванням) — просто показ оцінки, можна дробову (4.5 → 4½)
 * - interactive (v-model) — користувач клікає на зірку, щоб поставити свою оцінку
 *
 * За дизайном Figma — золоті зірки (--color-star) на тлі сторінки.
 */

const props = withDefaults(
  defineProps<{
    modelValue?: number;
    max?: number;
    readonly?: boolean;
    size?: number;
  }>(),
  {
    modelValue: 0,
    max: 5,
    readonly: false,
    size: 28,
  }
);

const emit = defineEmits<{
  'update:modelValue': [value: number];
}>();

const hoveredValue = ref(0);

// Що показуємо: hover (для interactive) або модельне значення.
const displayValue = computed(() => {
  if (!props.readonly && hoveredValue.value > 0) return hoveredValue.value;
  return props.modelValue;
});

function getFill(starIndex: number): 'full' | 'half' | 'empty' {
  // starIndex 1..max
  const v = displayValue.value;
  if (v >= starIndex) return 'full';
  if (v >= starIndex - 0.5) return 'half';
  return 'empty';
}

function onClick(starIndex: number): void {
  if (props.readonly) return;
  // Якщо клікнули по тій самій зірці — знімаємо оцінку.
  const next = props.modelValue === starIndex ? 0 : starIndex;
  emit('update:modelValue', next);
}

function onHover(starIndex: number): void {
  if (props.readonly) return;
  hoveredValue.value = starIndex;
}

function onLeave(): void {
  hoveredValue.value = 0;
}
</script>

<template>
  <div
    class="star-rating"
    :class="{ 'star-rating--readonly': readonly }"
    role="img"
    :aria-label="`Оцінка ${modelValue} з ${max}`"
    @mouseleave="onLeave"
  >
    <button
      v-for="i in max"
      :key="i"
      type="button"
      class="star-rating__btn"
      :disabled="readonly"
      :aria-label="`${i} ${i === 1 ? 'зірка' : 'зірок'}`"
      @click="onClick(i)"
      @mouseenter="onHover(i)"
    >
      <svg
        :width="size"
        :height="size"
        viewBox="0 0 24 24"
        :class="['star-rating__icon', `star-rating__icon--${getFill(i)}`]"
        aria-hidden="true"
      >
        <defs>
          <linearGradient :id="`star-half-${i}`">
            <stop offset="50%" stop-color="currentColor" />
            <stop offset="50%" stop-color="transparent" stop-opacity="0" />
          </linearGradient>
        </defs>
        <path
          d="M12 2L14.39 8.55L21 9.27L16 14.14L17.18 21L12 17.77L6.82 21L8 14.14L3 9.27L9.61 8.55L12 2Z"
          :fill="getFill(i) === 'full' ? 'currentColor' : getFill(i) === 'half' ? `url(#star-half-${i})` : 'none'"
          stroke="currentColor"
          stroke-width="1.5"
          stroke-linejoin="round"
        />
      </svg>
    </button>
  </div>
</template>

<style scoped>
.star-rating {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: var(--color-star);
}

.star-rating__btn {
  background: transparent;
  border: none;
  padding: 0;
  cursor: pointer;
  display: inline-flex;
  line-height: 0;
  color: inherit;
  transition: transform 0.1s;
}

.star-rating--readonly .star-rating__btn {
  cursor: default;
}

.star-rating__btn:not(:disabled):hover {
  transform: scale(1.1);
}

.star-rating__btn:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
  border-radius: 4px;
}
</style>
