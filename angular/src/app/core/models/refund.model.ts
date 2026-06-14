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
  totalCount: number;
}

export interface RefundLiabilityLookup {
  reservationId: string;
  reservationNumber: string;
  customerName: string;
  hallName: string;
  paidAmount: number;
  depositAmount: number;
  installmentsPaid: number;
  refundableAmount: number;
  liabilityAmount: number;
  liabilityJournalEntryId?: string | null;
  liabilityJournalEntryNumber?: string | null;
  statusCode: string;
  status: string;
  cancelledAt?: string | null;
}

export interface ProcessRefundResult {
  reservationId: string;
  journalEntryId: string;
  entryNumber: string;
  refundAmount: number;
}

export interface RefundDetails {
  reservationId: string;
  reservationNumber: string;
  reservationDate: string;
  eventDate: string;
  startTime: string;
  hallName: string;
  reservationStatus: string;
  customerName: string;
  customerPhone: string;
  totalPrice: number;
  depositAmount: number;
  installmentsPaid: number;
  paidAmount: number;
  refundableAmount: number;
  remainingBalance: number;
  liabilityAmount: number;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
  refundStatusCode: string;
  refundStatus: string;
  isRefundEligible: boolean;
  processedBy?: string | null;
  processedAt?: string | null;
  liabilityJournalEntryId?: string | null;
  liabilityJournalEntryNumber?: string | null;
  refundJournalEntryId?: string | null;
  refundJournalEntryNumber?: string | null;
  payments?: RefundPaymentHistoryItem[];
}

export interface RefundPaymentHistoryItem {
  id: string;
  amount: number;
  paymentDate: string;
  paymentType: string;
  receiptNumber: string;
}
