export interface ReservationFilter {
  hallId: string | null;
  customerId: string | null;
  eventDateFrom: string | null;
  eventDateTo: string | null;
  reservationNumber: string | null;
  status: string | null;
}

export const EMPTY_RESERVATION_FILTER: ReservationFilter = {
  hallId: null,
  customerId: null,
  eventDateFrom: null,
  eventDateTo: null,
  reservationNumber: null,
  status: null,
};
