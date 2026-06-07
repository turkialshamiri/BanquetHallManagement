export const RESERVATION_STATUS = {
  Pending: 'Pending',
  Confirmed: 'Confirmed',
  Cancelled: 'Cancelled',
  Completed: 'Completed',
} as const;

export type ReservationStatusValue =
  (typeof RESERVATION_STATUS)[keyof typeof RESERVATION_STATUS];

const STATUS_LABELS: Record<string, string> = {
  Pending: 'قيد الانتظار',
  Confirmed: 'مؤكد',
  Cancelled: 'ملغي',
  Completed: 'مكتمل',
};

export function getReservationStatusLabel(status: string): string {
  if (!status) {
    return '—';
  }

  return STATUS_LABELS[status] ?? status;
}

export function getReservationStatusClass(status: string): string {
  if (!status) {
    return 'status-unknown';
  }

  return `status-${status.toLowerCase()}`;
}

export function canConfirmReservation(status: string): boolean {
  return status === RESERVATION_STATUS.Pending;
}

export function canCancelReservation(status: string): boolean {
  return (
    status === RESERVATION_STATUS.Pending ||
    status === RESERVATION_STATUS.Confirmed
  );
}

export function canCompleteReservation(status: string): boolean {
  return status === RESERVATION_STATUS.Confirmed;
}

export function canEditReservation(status: string): boolean {
  return (
    status === RESERVATION_STATUS.Pending ||
    status === RESERVATION_STATUS.Confirmed
  );
}

export function canDeleteReservation(status: string): boolean {
  return status === RESERVATION_STATUS.Pending;
}
