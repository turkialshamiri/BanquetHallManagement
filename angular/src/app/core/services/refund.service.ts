import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import {
  PendingRefundListResult,
  ProcessRefundResult,
} from '../models/refund.model';

@Injectable({ providedIn: 'root' })
export class RefundService {
  private http = inject(HttpClient);
  private apiUrl = `${API_APP_BASE}/refund`;

  getPending(): Observable<PendingRefundListResult> {
    return this.http.get<PendingRefundListResult>(`${this.apiUrl}/pending`);
  }

  process(reservationId: string): Observable<ProcessRefundResult> {
    return this.http.post<ProcessRefundResult>(
      `${this.apiUrl}/process/${reservationId}`,
      {}
    );
  }
}
