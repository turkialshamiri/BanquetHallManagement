export interface Payment {
  id: string;
  reservationId: string;
  amount: number;
  paymentDate: string;
  paymentType: string;
  receiptNumber: string;
}

export interface InstallmentPaymentResult extends Payment {
  remainingAmount: number;
  isFullyPaid: boolean;
  hallAccessCardId?: string | null;
}

export interface RecordDepositInput {
  reservationId: string;
  amount: number;
}

export interface RecordInstallmentInput {
  reservationId: string;
  amount: number;
}

export interface PaymentListResult {
  items: Payment[];
}
