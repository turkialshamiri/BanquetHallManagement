import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import {
  CreateUpdateService,
  PagedServiceResult,
  ServiceItem,
} from '../models/service.model';

@Injectable({
  providedIn: 'root',
})
export class ServiceService {
  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:44324/api/app/service';

  getServices(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT
  ): Observable<PagedServiceResult> {
    const params = new HttpParams()
      .set('skipCount', skipCount)
      .set('maxResultCount', maxResultCount);

    return this.http.get<PagedServiceResult>(this.apiUrl, { params });
  }

  createService(data: CreateUpdateService): Observable<ServiceItem> {
    return this.http.post<ServiceItem>(this.apiUrl, data);
  }

  updateService(
    id: string,
    data: CreateUpdateService
  ): Observable<ServiceItem> {
    return this.http.put<ServiceItem>(`${this.apiUrl}/${id}`, data);
  }

  deleteService(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
