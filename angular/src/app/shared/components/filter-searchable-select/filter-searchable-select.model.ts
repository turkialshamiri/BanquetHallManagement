export interface FilterSearchableSelectItem {
  id: string;
  label: string;
  /** Additional text included in search matching (e.g. phone number). */
  searchText?: string;
}
