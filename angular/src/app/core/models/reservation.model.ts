export interface Reservation {
  id: string;
  hallId: string;
  customerId: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  guestsCount: number;
  totalPrice: number;
  status: string;
  serviceIds?: string[] | null;
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
