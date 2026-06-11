export interface HallAccessCard {
  id: string;
  reservationId: string;
  cardNumber: string;
  issuedAt: string;
  eventDate: string;
  entryTime: string;
  exitTime: string;
  isUsed: boolean;
}

export interface HallAccessCardPrintData {
  hallAccessCardId: string;
  cardNumber: string;
  issuedAt: string;
  reservationId: string;
  eventDate: string;
  entryTime: string;
  exitTime: string;
  guestsCount: number;
  customerName: string;
  customerPhone: string;
  customerCompany?: string | null;
  hallName: string;
  hallLocation: string;
}
