/**
 * Normalizes text for case-insensitive, Arabic-friendly client-side search.
 */
export function normalizeSearchText(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[أإآٱا]/g, 'ا')
    .replace(/ى/g, 'ي')
    .replace(/ة/g, 'ه')
    .replace(/\s+/g, ' ');
}

export function matchesSearchQuery(
  source: string,
  query: string
): boolean {
  const normalizedQuery = normalizeSearchText(query);

  if (!normalizedQuery) {
    return true;
  }

  return normalizeSearchText(source).includes(normalizedQuery);
}
