export interface Reservation {
  id: string;
  reservationNumber: string;
  hallId: string;
  customerId: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  guestsCount: number;
  totalPrice: number;
  paidAmount?: number;
  remainingAmount?: number;
  status: string;
  completedAt?: string | null;
  serviceIds?: string[] | null;
  cancellationReason?: string | null;
  cancellationType?: string | null;
  creationTime?: string;
  lastModificationTime?: string | null;
  createdBy?: string | null;
  lastModifiedBy?: string | null;
}

export interface CreateUpdateReservation {
  hallId: string;
  customerId: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  guestsCount: number;
  serviceIds?: string[] | null;
}

export interface PagedReservationResult {
  items: Reservation[];
  totalCount: number;
}

export function toDateInputValue(isoDate: string): string {
  if (!isoDate) {
    return '';
  }

  return isoDate.split('T')[0];
}

export function toTimeInputValue(timeSpan: string): string {
  if (!timeSpan) {
    return '';
  }

  const parts = timeSpan.split(':');
  const hours = parts[0]?.padStart(2, '0') ?? '00';
  const minutes = parts[1]?.padStart(2, '0') ?? '00';

  return `${hours}:${minutes}`;
}

export function toApiTimeSpan(timeInput: string): string {
  if (!timeInput) {
    return '';
  }

  const segments = timeInput.split(':');

  if (segments.length === 2) {
    return `${timeInput}:00`;
  }

  return timeInput;
}

export function toApiEventDate(dateInput: string): string {
  if (!dateInput) {
    return '';
  }

  return dateInput.includes('T') ? dateInput : `${dateInput}T00:00:00`;
}

export function calculateDurationLabel(startTime: string, endTime: string): string {
  if (!startTime || !endTime) {
    return '—';
  }

  const [startHours, startMinutes] = startTime.split(':').map(Number);
  const [endHours, endMinutes] = endTime.split(':').map(Number);
  const startTotal = startHours * 60 + (startMinutes || 0);
  const endTotal = endHours * 60 + (endMinutes || 0);
  const diff = Math.max(0, endTotal - startTotal);
  const hours = Math.floor(diff / 60);
  const minutes = diff % 60;

  if (hours > 0 && minutes > 0) {
    return `${hours}h ${minutes}m`;
  }

  if (hours > 0) {
    return `${hours}h`;
  }

  return `${minutes}m`;
}

export function resolvePaymentStatusCode(
  paidAmount: number,
  totalPrice: number
): 'FullyPaid' | 'PartiallyPaid' | 'Unpaid' {
  if (!totalPrice || paidAmount <= 0) {
    return 'Unpaid';
  }

  if (paidAmount >= totalPrice) {
    return 'FullyPaid';
  }

  return 'PartiallyPaid';
}
