import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import {
  CreateUpdateReservation,
  PagedReservationResult,
  Reservation,
} from '../models/reservation.model';

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  private http = inject(HttpClient);

  private apiUrl = `${API_APP_BASE}/reservation`;

  getReservations(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT
  ): Observable<PagedReservationResult> {
    const params = new HttpParams()
      .set('skipCount', skipCount)
      .set('maxResultCount', maxResultCount);

    return this.http.get<PagedReservationResult>(this.apiUrl, { params });
  }

  createReservation(
    data: CreateUpdateReservation
  ): Observable<Reservation> {
    return this.http.post<Reservation>(this.apiUrl, data);
  }

  updateReservation(
    id: string,
    data: CreateUpdateReservation
  ): Observable<Reservation> {
    return this.http.put<Reservation>(`${this.apiUrl}/${id}`, data);
  }

  deleteReservation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  confirmReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.apiUrl}/${id}/confirm`,
      {}
    );
  }

  cancelReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.apiUrl}/${id}/cancel`,
      {}
    );
  }

  completeReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.apiUrl}/${id}/complete`,
      {}
    );
  }

  confirmHallEntry(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.apiUrl}/${id}/confirm-hall-entry`,
      {}
    );
  }
}
