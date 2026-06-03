
export interface CreateUpdateReservationDto {
  hallId: string;
  customerId: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  guestsCount: number;
  serviceIds?: string[];
}

export interface ReservationDto {
  id?: string;
  hallId?: string;
  customerId?: string;
  eventDate?: string;
  startTime?: string;
  endTime?: string;
  guestsCount?: number;
  totalPrice?: number;
  status?: string;
  serviceIds?: string[];
}
