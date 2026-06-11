export interface PendingRefund {
  reservationId: string;
  customerId: string;
  customerName: string;
  eventDate: string;
  startTime: string;
  totalPrice: number;
  paidAmount: number;
  refundAmount: number;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
}

export interface PendingRefundListResult {
  items: PendingRefund[];
}

export interface ProcessRefundResult {
  reservationId: string;
  journalEntryId: string;
  entryNumber: string;
  refundAmount: number;
}
