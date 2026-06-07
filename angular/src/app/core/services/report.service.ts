import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReportFilters, ReportsResult } from '../models/report.model';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:44324/api/app/reports';

  getReports(filters: ReportFilters): Observable<ReportsResult> {
    let params = new HttpParams()
      .set('dateFrom', filters.dateFrom)
      .set('dateTo', filters.dateTo);

    if (filters.hallId) {
      params = params.set('hallId', filters.hallId);
    }

    if (filters.status != null) {
      params = params.set('status', filters.status.toString());
    }

    return this.http.get<ReportsResult>(this.apiUrl, { params });
  }
}
