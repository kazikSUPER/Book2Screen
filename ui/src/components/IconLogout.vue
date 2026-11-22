<script setup lang="ts">
/**
 * Іконка "Вийти" — стрілочка з квадратика назовні.
 * Показується замість IconUser у хедері коли користувач на сторінці профілю.
 * При кліку (обробник у App.vue) — виконує logout.
 */
withDefaults(defineProps<{ size?: number }>(), { size: 28 });
</script>

<template>
  <svg
    :width="size"
    :height="size"
    viewBox="0 0 24 24"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    aria-hidden="true"
  >
    <!-- Рамка (контур двері) -->
    <path
      d="M10 17v2a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    />
    <!-- Стрілка вправо -->
    <path
      d="M16 17l5-5-5-5M21 12H9"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    />
  </svg>
</template>
