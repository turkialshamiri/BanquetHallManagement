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
