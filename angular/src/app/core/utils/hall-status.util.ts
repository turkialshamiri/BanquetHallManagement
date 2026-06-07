export const HALL_STATUS = {
  Available: 1,
  Maintenance: 2,
  Booked: 3,
} as const;

export type HallStatusValue =
  (typeof HALL_STATUS)[keyof typeof HALL_STATUS];

export type OperationalHallStatus =
  | typeof HALL_STATUS.Available
  | typeof HALL_STATUS.Maintenance;

const STATUS_LABELS: Record<number, string> = {
  [HALL_STATUS.Available]: 'متاحة',
  [HALL_STATUS.Maintenance]: 'تحت الصيانة',
  [HALL_STATUS.Booked]: 'محجوزة',
};

const STATUS_CLASSES: Record<number, string> = {
  [HALL_STATUS.Available]: 'available',
  [HALL_STATUS.Maintenance]: 'maintenance',
  [HALL_STATUS.Booked]: 'booked',
};

export function getHallStatusLabel(status: number): string {
  return STATUS_LABELS[status] ?? 'غير معروف';
}

export function getHallStatusClass(status: number): string {
  return STATUS_CLASSES[status] ?? 'unknown';
}

export function getOperationalStatusLabel(status: number): string {
  if (status === HALL_STATUS.Maintenance) {
    return STATUS_LABELS[HALL_STATUS.Maintenance];
  }

  return STATUS_LABELS[HALL_STATUS.Available];
}
