/** Must match [LocalizationResourceName] on BanquetHallManagementResource. */
export const DEFAULT_L10N_RESOURCE = 'BanquetHallManagement';

/**
 * ABP LocalizationService requires keys as `Resource::Key` or `::Key` (default resource).
 * Keys like `Navbar:Title` are returned unchanged and appear literally in the UI.
 */
export function toAbpL10nKey(key: string): string {
  if (!key) {
    return key;
  }

  if (key.includes('::')) {
    return key;
  }

  return `::${key}`;
}
