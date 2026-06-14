import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_APP_BASE } from '../constants/api.constants';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import { createPagingParams } from '../utils/pagination.util';
import {
  JournalEntry,
  PagedJournalEntryResult,
} from '../models/journal-entry.model';

@Injectable({ providedIn: 'root' })
export class JournalEntryService {
  private http = inject(HttpClient);
  private apiUrl = `${API_APP_BASE}/journal-entry`;

  getList(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT
  ): Observable<PagedJournalEntryResult> {
    const params = createPagingParams(skipCount, maxResultCount);

    return this.http.get<PagedJournalEntryResult>(this.apiUrl, { params });
  }

  get(id: string): Observable<JournalEntry> {
    return this.http.get<JournalEntry>(`${this.apiUrl}/${id}`);
  }
}
