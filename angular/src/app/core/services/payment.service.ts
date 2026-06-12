import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import {
  DepositPaymentResult,
  InstallmentPaymentResult,
  PaymentListResult,
  RecordDepositInput,
  RecordInstallmentInput,
} from '../models/payment.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = `${API_APP_BASE}/payment`;

  recordDeposit(input: RecordDepositInput): Observable<DepositPaymentResult> {
    return this.http.post<DepositPaymentResult>(`${this.apiUrl}/record-deposit`, input);
  }

  recordInstallment(input: RecordInstallmentInput): Observable<InstallmentPaymentResult> {
    return this.http.post<InstallmentPaymentResult>(
      `${this.apiUrl}/record-installment`,
      input
    );
  }

  getByReservation(reservationId: string): Observable<PaymentListResult> {
    return this.http.get<PaymentListResult>(
      `${this.apiUrl}/by-reservation/${reservationId}`
    );
  }
}
