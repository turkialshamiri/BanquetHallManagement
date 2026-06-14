import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import { DEFAULT_PAGE_SIZE } from '../constants/pagination.constants';
import { createPagingParams } from '../utils/pagination.util';
import {
  PendingRefundListResult,
  ProcessRefundResult,
  RefundDetails,
  RefundLiabilityLookup,
} from '../models/refund.model';

@Injectable({ providedIn: 'root' })
export class RefundService {
  private http = inject(HttpClient);
  private apiUrl = `${API_APP_BASE}/refund`;

  getPending(
    filter?: string,
    skipCount = 0,
    maxResultCount = DEFAULT_PAGE_SIZE
  ): Observable<PendingRefundListResult> {
    const params = createPagingParams(skipCount, maxResultCount, undefined, {
      filter: filter?.trim(),
    });

    return this.http.get<PendingRefundListResult>(`${this.apiUrl}/pending`, {
      params,
    });
  }

  getDetails(reservationId: string): Observable<RefundDetails> {
    return this.http.get<RefundDetails>(`${this.apiUrl}/details/${reservationId}`);
  }

  getByReservationNumber(reservationNumber: string): Observable<RefundLiabilityLookup> {
    return this.http.get<RefundLiabilityLookup>(
      `${this.apiUrl}/by-reservation-number/${encodeURIComponent(reservationNumber)}`
    );
  }

  process(reservationId: string): Observable<ProcessRefundResult> {
    return this.http.post<ProcessRefundResult>(
      `${this.apiUrl}/process/${reservationId}`,
      {}
    );
  }

  processByReservationNumber(reservationNumber: string): Observable<ProcessRefundResult> {
    return this.http.post<ProcessRefundResult>(
      `${this.apiUrl}/process-by-reservation-number/${encodeURIComponent(reservationNumber)}`,
      {}
    );
  }
}
