import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  private apiUrl = 'https://localhost:44324/api/app/reservation';

  getReservations(): Observable<PagedReservationResult> {
    return this.http.get<PagedReservationResult>(this.apiUrl);
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
}
