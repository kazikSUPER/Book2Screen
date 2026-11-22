<script setup lang="ts">
/**
 * Логотип Book2Screen — стилізована композиція "Book / t / Screen"
 * з декоративними іконками, як у Figma. Шрифт — рукописний (--font-logo).
 *
 * Використання:
 * <Logo />             — стандартний (для шапки, ~80px)
 * <Logo size="sm" />   — компактний
 * <Logo size="lg" />   — великий (для спец. сторінок)
 */

withDefaults(
  defineProps<{
    size?: 'sm' | 'md' | 'lg';
  }>(),
  { size: 'md' }
);
</script>

<template>
  <span class="logo" :class="`logo--${size}`" aria-label="Book2Screen">
    <span class="logo__row logo__row--top">
      <span class="logo__word">Book</span>
      <svg
        class="logo__icon"
        viewBox="0 0 20 20"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
        aria-hidden="true"
      >
        <!-- Ліва сторінка -->
        <rect x="0.5" y="0.5" width="3" height="19" rx="0.5" />
        <line x1="3.5" y1="0.5" x2="3.5" y2="19.5" />
        <!-- Права сторінка -->
        <rect x="3.5" y="0.5" width="5.5" height="19" rx="0.5" />
        <!-- Косий аркуш (відкрита сторінка) -->
        <path d="M11.2 2.8L13 2.3L17 17L15.2 17.5L11.2 2.8Z" />
      </svg>
    </span>

    <span class="logo__row logo__row--mid">
      <span class="logo__word">t</span>
      <svg class="logo__icon" viewBox="0 0 19 19" fill="currentColor" aria-hidden="true">
        <path
          d="M16.5916 3.19883C14.8549 1.23945 12.3277 0 9.5 0C4.25273 0 0 4.25273 0 9.5H2.375C2.375 5.5627 5.5627 2.375 9.5 2.375C11.6709 2.375 13.5969 3.3584 14.8994 4.89102L12.6654 7.125H19V0.79043L16.5916 3.19883ZM9.5 16.625C7.3291 16.625 5.40312 15.6416 4.10059 14.109L6.33457 11.875H0V18.2096L2.4084 15.8012C4.14512 17.7605 6.67598 19 9.5 19C14.7473 19 19 14.7473 19 9.5H16.625C16.625 13.4373 13.4373 16.625 9.5 16.625Z"
        />
      </svg>
    </span>

    <span class="logo__row logo__row--bottom">
      <svg class="logo__icon" viewBox="0 0 24 23" fill="currentColor" aria-hidden="true">
        <path
          d="M1.66401 8.40583L0.0349536 17.5998C-0.0172599 17.8945 -0.0109578 18.1805 0.0538599 18.4579C0.118678 18.7354 0.229887 18.9868 0.38749 19.2123C0.545092 19.4378 0.743025 19.6286 0.981289 19.7848C1.21955 19.941 1.48602 20.0452 1.7807 20.0974L17.3397 22.8543C17.6344 22.9065 17.9205 22.9002 18.1979 22.8354C18.4753 22.7706 18.7268 22.6594 18.9522 22.5018C19.1777 22.3441 19.3685 22.1462 19.5247 21.908C19.6809 21.6697 19.7851 21.4032 19.8373 21.1085L20.8301 15.506C20.8784 15.2334 20.9184 14.9859 20.9502 14.7636C20.982 14.5413 21.0131 14.3227 21.0436 14.1077C21.0741 13.8928 21.1096 13.6711 21.15 13.4427C21.1905 13.2144 21.249 12.9702 21.3254 12.7102C21.3676 12.5581 21.4145 12.4221 21.4661 12.302C21.5178 12.182 21.5695 12.062 21.6211 11.942C21.6728 11.822 21.7305 11.7106 21.7943 11.6079C21.8581 11.5053 21.9234 11.3725 21.9902 11.2096L23.3328 8.19921L4.97695 0.000308913L2.63357 5.24959L15.0019 10.7691L1.66401 8.40583ZM16.1152 6.54422L19.5058 8.05678L15.2714 9.31244L11.8809 7.79988L16.1152 6.54422ZM9.86224 6.90653L6.4626 5.38097L10.695 4.13636L14.0966 5.65087L9.86224 6.90653ZM21.4132 8.99881L20.2672 11.5425L17.2881 10.2168L21.4132 8.99881ZM5.70115 1.89522L8.67836 3.23196L4.56429 4.45195L5.70115 1.89522ZM2.82785 10.0709L19.8013 13.0784L18.4229 20.8579C18.389 21.0495 18.2896 21.2028 18.1248 21.318C17.96 21.4332 17.7819 21.4738 17.5903 21.4398L2.03133 18.683C1.83979 18.649 1.68643 18.5497 1.57126 18.3849C1.45609 18.2201 1.41547 18.042 1.44941 17.8504L2.82785 10.0709Z"
        />
      </svg>
      <span class="logo__word">Screen</span>
    </span>
  </span>
</template>

<style scoped>
.logo {
  display: inline-flex;
  flex-direction: column;
  font-family: var(--font-logo);
  color: #000000;
  background-color: var(--color-page);
  border: 2px solid var(--color-card);
  border-radius: var(--radius-md);
  line-height: 1;
  user-select: none;
  letter-spacing: 0.5px;
  justify-content: center;
  align-items: stretch;
}

.logo__row {
  display: flex;
  align-items: center;
  gap: 4px;
}

.logo__row--top {
  justify-content: flex-start;
}

.logo__row--mid {
  justify-content: center;
}

.logo__row--bottom {
  justify-content: flex-end;
  margin-right: -15px;
}

.logo__word {
  font-weight: 400;
  font-size: 1em;
  white-space: nowrap;
}

.logo__icon {
  width: 0.7em;
  height: 0.7em;
  display: inline-block;
  flex-shrink: 0;
  color: #000000;
}

/* Розміри */
.logo--sm {
  font-size: 18px;
  min-width: 190px;
  padding: 0px 18px;
}
.logo--md {
  font-size: 24px;
  min-width: 150px;
  padding: 0px 24px;
}
.logo--lg {
  font-size: 44px;
  min-width: 480px;
  padding: 0px 48px;
}

@media (max-width: 768px) {
  .logo--md {
    font-size: 20px;
    min-width: 200px;
    padding: 0px 18px;
  }
}

@media (max-width: 380px) {
  .logo--md {
    font-size: 18px;
    min-width: 170px;
    padding: 0px 14px;
  }
}
</style>
