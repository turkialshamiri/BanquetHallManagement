import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import { createPagingParams } from '../utils/pagination.util';
import {
  Invoice,
  InvoicePrintData,
  PagedInvoiceResult,
} from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private http = inject(HttpClient);
  private apiUrl = `${API_APP_BASE}/invoice`;

  getList(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT
  ): Observable<PagedInvoiceResult> {
    const params = createPagingParams(skipCount, maxResultCount);

    return this.http.get<PagedInvoiceResult>(this.apiUrl, { params });
  }

  get(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.apiUrl}/${id}`);
  }

  getByReservation(reservationId: string): Observable<{ items: Invoice[] }> {
    return this.http.get<{ items: Invoice[] }>(
      `${this.apiUrl}/by-reservation/${reservationId}`
    );
  }

  getSettlementByReservation(reservationId: string): Observable<Invoice> {
    return this.http.get<Invoice>(
      `${this.apiUrl}/settlement-by-reservation/${reservationId}`
    );
  }

  getPrintData(id: string): Observable<InvoicePrintData> {
    return this.http.get<InvoicePrintData>(`${this.apiUrl}/${id}/print-data`);
  }
}
