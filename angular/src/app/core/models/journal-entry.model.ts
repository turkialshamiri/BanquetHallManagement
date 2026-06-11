export interface JournalEntryLine {
  id: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  description?: string | null;
}

export interface JournalEntry {
  id: string;
  entryNumber: string;
  entryDate: string;
  sourceType: string;
  description?: string | null;
  reservationId?: string | null;
  paymentId?: string | null;
  isPosted: boolean;
  postedTime?: string | null;
  totalDebit: number;
  totalCredit: number;
  lines: JournalEntryLine[];
}

export interface PagedJournalEntryResult {
  items: JournalEntry[];
  totalCount: number;
}
