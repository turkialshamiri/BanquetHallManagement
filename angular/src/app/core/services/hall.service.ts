import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Hall, HallFormModel, PagedHallResult } from '../models/hall.model';

@Injectable({
  providedIn: 'root',
})
export class HallService {
  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:44324/api/app/hall';

  createHall(data: HallFormModel): Observable<Hall> {
    return this.http.post<Hall>(this.apiUrl, data);
  }

  updateHall(id: string, hall: HallFormModel): Observable<Hall> {
    return this.http.put<Hall>(`${this.apiUrl}/${id}`, hall);
  }

  getHalls(): Observable<PagedHallResult> {
    return this.http.get<PagedHallResult>(this.apiUrl);
  }

  getHallsByStatus(status: number): Observable<Hall[]> {
    return this.http.get<Hall[]>(`${this.apiUrl}/by-status`, {
      params: { status: status.toString() },
    });
  }

  getHallsList(statusFilter: number | null = null): Observable<Hall[]> {
    if (statusFilter != null) {
      return this.getHallsByStatus(statusFilter);
    }

    return this.getHalls().pipe(map((result) => result.items));
  }

  deleteHall(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
