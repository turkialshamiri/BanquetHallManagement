import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import { createPagingParams } from '../utils/pagination.util';
import {
  CreateUpdateReservation,
  PagedReservationResult,
  Reservation,
} from '../models/reservation.model';
import {
  EMPTY_RESERVATION_FILTER,
  ReservationFilter,
} from '../models/reservation-filter.model';

const RESERVATION_STATUS_API_VALUES: Record<string, number> = {
  Pending: 1,
  Confirmed: 2,
  Cancelled: 3,
  Completed: 4,
  FullyPaid: 5,
  Archived: 6,
};

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  private http = inject(HttpClient);

  private apiUrl = `${API_APP_BASE}/reservation`;

  getReservations(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT,
    filter: ReservationFilter = EMPTY_RESERVATION_FILTER
  ): Observable<PagedReservationResult> {
    const params = createPagingParams(
      skipCount,
      maxResultCount,
      undefined,
      this.buildFilterQueryParams(filter)
    );

    return this.http.get<PagedReservationResult>(this.apiUrl, { params });
  }

  private buildFilterQueryParams(
    filter: ReservationFilter
  ): Record<string, string | number | boolean | null | undefined> {
    const params: Record<string, string | number | boolean | null | undefined> = {};

    if (filter.hallId) {
      params.hallId = filter.hallId;
    }

    if (filter.customerId) {
      params.customerId = filter.customerId;
    }

    if (filter.eventDateFrom) {
      params.eventDateFrom = filter.eventDateFrom;
    }

    if (filter.eventDateTo) {
      params.eventDateTo = filter.eventDateTo;
    }

    const reservationNumber = filter.reservationNumber?.trim();
    if (reservationNumber) {
      params.reservationNumber = reservationNumber;
    }

    if (filter.status) {
      params.status =
        RESERVATION_STATUS_API_VALUES[filter.status] ?? filter.status;
    }

    return params;
  }

  getReservation(id: string): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.apiUrl}/${id}`);
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

  archiveReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/${id}/archive`, {});
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

  confirmHallEntry(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.apiUrl}/${id}/confirm-hall-entry`,
      {}
    );
  }

  confirmHallEntryByReservationNumber(
    reservationNumber: string
  ): Observable<Reservation> {
    return this.http.post<Reservation>(
      `${this.apiUrl}/confirm-hall-entry-by-reservation-number`,
      { reservationNumber }
    );
  }

  getByReservationNumber(reservationNumber: string): Observable<Reservation> {
    return this.http.get<Reservation>(
      `${this.apiUrl}/by-reservation-number/${encodeURIComponent(reservationNumber)}`
    );
  }
}
