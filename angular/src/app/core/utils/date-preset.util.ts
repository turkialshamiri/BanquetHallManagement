export type DatePresetId =
  | 'today'
  | 'thisWeek'
  | 'thisMonth'
  | 'last30Days'
  | 'thisYear'
  | 'all';

export interface DatePresetRange {
  from: Date | null;
  to: Date | null;
}

function startOfDay(date: Date): Date {
  const value = new Date(date);
  value.setHours(0, 0, 0, 0);
  return value;
}

function startOfWeek(date: Date): Date {
  const value = startOfDay(date);
  const day = value.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  value.setDate(value.getDate() + diff);
  return value;
}

function endOfWeek(date: Date): Date {
  const value = startOfWeek(date);
  value.setDate(value.getDate() + 6);
  return value;
}

function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

function endOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth() + 1, 0);
}

function startOfYear(date: Date): Date {
  return new Date(date.getFullYear(), 0, 1);
}

function endOfYear(date: Date): Date {
  return new Date(date.getFullYear(), 11, 31);
}

export function resolveDatePreset(preset: DatePresetId, reference = new Date()): DatePresetRange {
  const today = startOfDay(reference);

  switch (preset) {
    case 'today':
      return { from: today, to: today };
    case 'thisWeek':
      return { from: startOfWeek(today), to: endOfWeek(today) };
    case 'thisMonth':
      return { from: startOfMonth(today), to: endOfMonth(today) };
    case 'last30Days': {
      const from = new Date(today);
      from.setDate(from.getDate() - 29);
      return { from, to: today };
    }
    case 'thisYear':
      return { from: startOfYear(today), to: endOfYear(today) };
    case 'all':
    default:
      return { from: null, to: null };
  }
}

export function formatDateForFilter(date: Date | null): string | null {
  if (!date) {
    return null;
  }

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

export function parseFilterDate(value: string | null): Date | null {
  if (!value) {
    return null;
  }

  const [year, month, day] = value.split('-').map(Number);
  if (!year || !month || !day) {
    return null;
  }

  return new Date(year, month - 1, day);
}
