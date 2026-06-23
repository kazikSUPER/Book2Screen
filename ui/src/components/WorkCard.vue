<script setup lang="ts">
import { useRouter } from 'vue-router';
import type { BookScreenItem } from '../services/types';
import { STR } from '../constants';
import { onImgError } from '../composables/useImageFallback';

const t = STR.detail;

/**
 * Картка твору — використовується на Home (карусель) і Top (сітка).
 * Рендериться у єдиному стилі, як у Figma:
 * — темно-винний фон, білий бордер, заокруглення 10px, тінь 5/4/4
 * — постер 200×260, заголовок Inter, мета-поля Inter, кнопка JetBrains Mono
 *
 * Клік на саму картку (за межами кнопки) і клік на кнопку — обидва ведуть
 * на сторінку деталей.
 */

const props = defineProps<{
  item: BookScreenItem;
}>();

const router = useRouter();

function goToDetail(): void {
  router.push({ name: 'detail', params: { id: props.item.id } });
}
</script>

<template>
  <article
    class="work-card"
    tabindex="0"
    role="button"
    :aria-label="`${item.title}, ${item.year}`"
    @click="goToDetail"
    @keydown.enter="goToDetail"
    @keydown.space.prevent="goToDetail"
  >
    <div class="work-card__poster">
      <img :src="item.poster" :alt="item.title" loading="lazy" @error="onImgError" />
    </div>

    <div class="work-card__info">
      <h3 class="work-card__title">{{ item.title }}</h3>
      <p class="work-card__meta">{{ t.bookYear }} {{ item.year }}</p>
      <p class="work-card__meta">{{ t.bookGenre }} {{ item.genre }}</p>
      <p class="work-card__meta">{{ t.bookCountry }} {{ item.country }}</p>
    </div>

    <button type="button" class="work-card__btn" @click.stop="goToDetail">{{ STR.profile.view }}</button>
  </article>
</template>

<style scoped>
.work-card {
  background-color: var(--color-card);
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  width: 220px;
  min-height: 360px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  cursor: pointer;
  transition: transform 0.2s;
  padding: 12px;
  gap: 10px;
}

.work-card:hover {
  transform: translateY(-2px);
}

.work-card__poster {
  width: 100%;
  height: 220px;
  overflow: hidden;
  background: var(--color-header);
  border: 1px solid var(--border-card);
  flex-shrink: 0;
  border-radius: var(--radius-xs);
}

.work-card__poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.work-card__info {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  padding: 0 4px;
}

.work-card__title {
  font-size: 14px;
  font-weight: 400;
  font-family: var(--font-body);
  color: var(--text-on-dark);
  margin: 0 0 6px 0;
  line-height: 1.3;
  text-align: center;
}

.work-card__meta {
  font-size: 12px;
  color: var(--text-on-dark);
  margin: 0;
  font-family: var(--font-body);
  line-height: 1.4;
}

.work-card__btn {
  width: 100%;
  height: 38px;
  background-color: var(--color-primary);
  color: var(--text-on-primary);
  border: 2px solid var(--color-primary-dark);
  border-radius: var(--radius-md);
  font-size: 14px;
  font-family: var(--font-display);
  cursor: pointer;
  box-shadow: var(--shadow-md);
  transition: background 0.2s;
  flex-shrink: 0;
}

.work-card__btn:hover {
  background-color: var(--color-primary-hover);
}

@media (max-width: 768px) {
  .work-card {
    width: 176px;
    min-height: 300px;
    padding: 10px;
  }
  .work-card__poster {
    height: 180px;
  }
}
</style>
