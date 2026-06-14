export interface Invoice {
  id: string;
  invoiceNumber: string;
  reservationId: string;
  paymentId: string;
  invoiceType: string;
  amount: number;
  issuedAt: string;
}

export interface PagedInvoiceResult {
  items: Invoice[];
  totalCount: number;
}

export interface InvoicePrintData {
  invoiceId: string;
  invoiceNumber: string;
  invoiceType: string;
  amount: number;
  issuedAt: string;
  receiptNumber: string;
  paymentDate: string;
  reservationId: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  guestsCount: number;
  totalPrice: number;
  paidAmount: number;
  reservationStatus: string;
  customerName: string;
  customerPhone: string;
  customerCompany?: string | null;
  hallName: string;
  hallLocation: string;
  reservationNumber: string;
  reservationCreatedAt: string;
  employeeName: string;
  companyName: string;
}
