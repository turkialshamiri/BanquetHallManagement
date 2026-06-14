import { Customer } from 'src/app/core/models/customer.model';
import { Hall } from 'src/app/core/models/hall.model';
import { Payment } from 'src/app/core/models/payment.model';
import { Reservation } from 'src/app/core/models/reservation.model';
import { ServiceItem } from 'src/app/core/models/service.model';

export interface ReservationServiceLine {
  serviceId: string;
  name: string;
  quantity: number;
  price: number;
  total: number;
}

export interface ReservationDetailsViewModel {
  reservation: Reservation;
  customer: Customer | null;
  hall: Hall | null;
  payments: Payment[];
  depositAmount: number;
  paymentStatusCode: 'FullyPaid' | 'PartiallyPaid' | 'Unpaid';
  services: ReservationServiceLine[];
  durationLabel: string;
}

export interface ReservationDetailsDialogData {
  details: ReservationDetailsViewModel;
  canArchive?: boolean;
}

export const RESERVATION_DETAILS_DIALOG_CONFIG = {
  width: 'min(860px, 95vw)',
  maxWidth: '95vw',
  maxHeight: '90dvh',
  panelClass: 'reservation-details-dialog-panel',
  disableClose: false,
} as const;
