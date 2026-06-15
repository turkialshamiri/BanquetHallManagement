export type ReservationFilterChipType =
  | 'hall'
  | 'customer'
  | 'date'
  | 'status'
  | 'reservationNumber';

export interface ReservationFilterChip {
  type: ReservationFilterChipType;
  label: string;
}
