/**
 * Утиліта: інлайн SVG-плейсхолдер для постерів, які не завантажились.
 *
 * Використання у шаблоні:
 *   <img :src="item.poster" @error="onImgError" />
 *
 * Або імпортуй POSTER_PLACEHOLDER напряму, якщо треба як default.
 */

export const POSTER_PLACEHOLDER =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" width="200" height="260" viewBox="0 0 200 260">
      <rect width="200" height="260" fill="#3d0f1a"/>
      <text x="100" y="125" font-family="sans-serif" font-size="40" fill="#f7cccc" text-anchor="middle">📕</text>
      <text x="100" y="170" font-family="sans-serif" font-size="13" fill="#f7cccc" text-anchor="middle">Постер</text>
      <text x="100" y="190" font-family="sans-serif" font-size="13" fill="#f7cccc" text-anchor="middle">недоступний</text>
    </svg>`
  );

/**
 * Колбек для @error на <img>. Підставляє плейсхолдер і знімає обробник
 * (щоб не залупитися, якщо плейсхолдер теж кине error).
 */
export function onImgError(e: Event): void {
  const img = e.target as HTMLImageElement;
  if (img.src === POSTER_PLACEHOLDER) return;
  img.src = POSTER_PLACEHOLDER;
  img.onerror = null;
}
