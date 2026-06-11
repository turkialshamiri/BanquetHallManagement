import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import {
  HallAccessCard,
  HallAccessCardPrintData,
} from '../models/hall-access-card.model';

@Injectable({ providedIn: 'root' })
export class HallAccessCardService {
  private http = inject(HttpClient);
  private apiUrl = `${API_APP_BASE}/hall-access-card`;

  get(id: string): Observable<HallAccessCard> {
    return this.http.get<HallAccessCard>(`${this.apiUrl}/${id}`);
  }

  getByReservation(reservationId: string): Observable<HallAccessCard> {
    return this.http.get<HallAccessCard>(
      `${this.apiUrl}/by-reservation/${reservationId}`
    );
  }

  getPrintData(id: string): Observable<HallAccessCardPrintData> {
    return this.http.get<HallAccessCardPrintData>(
      `${this.apiUrl}/print-data/${id}`
    );
  }
}
