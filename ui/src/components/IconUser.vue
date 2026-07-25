<script setup lang="ts">
/**
 * Іконка користувача (силует) для кнопки авторизації у шапці.
 * Колір береться з CSS-властивості currentColor — задається ззовні.
 */
withDefaults(
  defineProps<{
    size?: number;
  }>(),
  { size: 28 }
);
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
    <!-- Голова -->
    <circle cx="12" cy="8" r="4" fill="currentColor" />
    <!-- Плечі / тулуб -->
    <path d="M4 21C4 16.5817 7.58172 13 12 13C16.4183 13 20 16.5817 20 21V22H4V21Z" fill="currentColor" />
  </svg>
</template>
