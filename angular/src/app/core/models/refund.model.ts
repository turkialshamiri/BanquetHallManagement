export interface PendingRefund {
  reservationId: string;
  reservationNumber: string;
  customerId: string;
  customerName: string;
  hallName: string;
  eventDate: string;
  startTime: string;
  totalPrice: number;
  paidAmount: number;
  depositAmount: number;
  installmentsPaid: number;
  refundableAmount: number;
  liabilityAmount: number;
  refundAmount: number;
  liabilityJournalEntryId?: string | null;
  liabilityJournalEntryNumber?: string | null;
  statusCode: string;
  status: string;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
}

export interface PendingRefundListResult {
  items: PendingRefund[];
}

export interface RefundLiabilityLookup {
  reservationId: string;
  reservationNumber: string;
  customerName: string;
  hallName: string;
  depositAmount: number;
  installmentsPaid: number;
  refundableAmount: number;
  liabilityAmount: number;
  liabilityJournalEntryId?: string | null;
  liabilityJournalEntryNumber?: string | null;
  statusCode: string;
  status: string;
}

export interface ProcessRefundResult {
  reservationId: string;
  journalEntryId: string;
  entryNumber: string;
  refundAmount: number;
}
