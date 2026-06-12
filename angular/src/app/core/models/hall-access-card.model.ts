export interface HallAccessCard {

  id: string;

  reservationId: string;

  reservationNumber: string;

  cardNumber: string;

  issuedAt: string;

  eventDate: string;

  entryTime: string;

  exitTime: string;

  isUsed: boolean;

}



export interface HallAccessCardListItem {

  id: string;

  reservationId: string;

  reservationNumber: string;

  cardNumber: string;

  issuedAt: string;

  eventDate: string;

  entryTime: string;

  exitTime: string;

  customerName: string;

  hallName: string;

  employeeName: string;

  reservationStatus: string;

  paymentStatus: string;

  isUsed: boolean;

}

export interface HallAccessCardEntryPreview {

  reservationId: string;

  reservationNumber: string;

  reservationStatus: string;

  paymentStatus: string;

  totalPrice: number;

  paidAmount: number;

  customerName: string;

  hallName: string;

  hallAccessCardId: string;

  cardNumber: string;

  eventDate: string;

  entryTime: string;

  exitTime: string;

  employeeName: string;

  canConfirmEntry: boolean;

}



export interface HallAccessCardPrintData {

  hallAccessCardId: string;

  cardNumber: string;

  issuedAt: string;

  reservationId: string;

  reservationNumber: string;

  eventDate: string;

  entryTime: string;

  exitTime: string;

  guestsCount: number;

  customerName: string;

  customerPhone: string;

  customerCompany?: string | null;

  hallName: string;

  hallLocation: string;

  employeeName: string;

}



export interface PagedHallAccessCardResult {

  items: HallAccessCardListItem[];

  totalCount: number;

}


