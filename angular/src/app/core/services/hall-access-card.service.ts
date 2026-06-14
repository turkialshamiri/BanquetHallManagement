import { Injectable, inject } from '@angular/core';

import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';

import { API_APP_BASE } from '../constants/api.constants';
import { DEFAULT_PAGE_SIZE } from '../constants/pagination.constants';
import { createPagingParams } from '../utils/pagination.util';

import {

  HallAccessCard,

  HallAccessCardEntryPreview,

  HallAccessCardPrintData,

  PagedHallAccessCardResult,

} from '../models/hall-access-card.model';



@Injectable({ providedIn: 'root' })

export class HallAccessCardService {

  private http = inject(HttpClient);

  private apiUrl = `${API_APP_BASE}/hall-access-card`;



  getList(
    skipCount = 0,
    maxResultCount = DEFAULT_PAGE_SIZE,
    reservationNumber?: string
  ): Observable<PagedHallAccessCardResult> {
    const params = createPagingParams(skipCount, maxResultCount, undefined, {
      reservationNumber: reservationNumber?.trim(),
    });

    return this.http.get<PagedHallAccessCardResult>(this.apiUrl, { params });
  }



  get(id: string): Observable<HallAccessCard> {

    return this.http.get<HallAccessCard>(`${this.apiUrl}/${id}`);

  }



  getByReservation(reservationId: string): Observable<HallAccessCard> {

    return this.http.get<HallAccessCard>(

      `${this.apiUrl}/by-reservation/${reservationId}`

    );

  }



  getByReservationNumber(reservationNumber: string): Observable<HallAccessCard> {

    return this.http.get<HallAccessCard>(

      `${this.apiUrl}/by-reservation-number/${encodeURIComponent(reservationNumber)}`

    );

  }

  getEntryPreviewByReservationNumber(
    reservationNumber: string
  ): Observable<HallAccessCardEntryPreview> {
    const params = new HttpParams().set(
      'reservationNumber',
      reservationNumber.trim()
    );

    return this.http.get<HallAccessCardEntryPreview>(
      `${this.apiUrl}/entry-preview-by-reservation-number`,
      { params }
    );
  }

  getEntryPreviewBySearch(search: string): Observable<HallAccessCardEntryPreview> {
    const params = new HttpParams().set('search', search.trim());

    return this.http.get<HallAccessCardEntryPreview>(
      `${this.apiUrl}/entry-preview-by-search`,
      { params }
    );
  }

  getPrintData(id: string): Observable<HallAccessCardPrintData> {

    return this.http.get<HallAccessCardPrintData>(
      `${this.apiUrl}/${id}/print-data`
    );

  }

}


