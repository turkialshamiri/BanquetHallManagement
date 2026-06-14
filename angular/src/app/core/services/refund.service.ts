import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
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

  getPending(filter?: string): Observable<PendingRefundListResult> {
    let params = new HttpParams();
    const trimmed = filter?.trim();

    if (trimmed) {
      params = params.set('filter', trimmed);
    }

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
