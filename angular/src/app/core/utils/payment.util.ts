import { RESERVATION_STATUS } from './reservation-status.util';

export const PAYMENT_RULES = {
  minimumDepositPercentage: 0.3,
  minimumInstallmentPercentage: 0.2,
} as const;

export function calculatePaymentPercentage(
  paidAmount: number,
  totalPrice: number
): number {
  if (!totalPrice || totalPrice <= 0) {
    return 0;
  }

  return Math.min(100, Math.round((paidAmount / totalPrice) * 100));
}

export function calculateMinimumDeposit(totalPrice: number): number {
  return totalPrice * PAYMENT_RULES.minimumDepositPercentage;
}

export function calculateMinimumInstallment(totalPrice: number): number {
  return totalPrice * PAYMENT_RULES.minimumInstallmentPercentage;
}

export function canRecordPayment(status: string, paidAmount: number, totalPrice: number): boolean {
  if (status === RESERVATION_STATUS.Pending && paidAmount < totalPrice) {
    return true;
  }

  if (status === RESERVATION_STATUS.Confirmed && paidAmount < totalPrice) {
    return true;
  }

  return false;
}

export function isDepositPayment(status: string, paidAmount: number): boolean {
  return status === RESERVATION_STATUS.Pending || paidAmount === 0;
}

export function getRemainingAmount(reservation: {
  totalPrice: number;
  paidAmount?: number;
  remainingAmount?: number;
}): number {
  if (reservation.remainingAmount != null) {
    return Math.max(0, reservation.remainingAmount);
  }

  const paid = reservation.paidAmount ?? 0;
  return Math.max(0, reservation.totalPrice - paid);
}
