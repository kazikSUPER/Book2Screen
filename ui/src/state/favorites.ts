/**
 * @deprecated
 * Цей стор більше не використовується. Логіку "Хочу прочитати / переглянути"
 * перенесено в `useWishlistStore` (state/wishlist.ts), бо там потрібен kind
 * (read | watch), а не просто список ID.
 *
 * Файл лишений, щоб не зламати імпорти, якщо ще десь є посилання.
 * Усі нові інтеграції — через useWishlistStore.
 */

export { useWishlistStore as useFavoritesStore } from './wishlist';
