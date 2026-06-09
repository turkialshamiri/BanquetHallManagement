import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import {
  CreateUpdateCustomer,
  Customer,
  PagedCustomerResult,
} from '../models/customer.model';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:44324/api/app/customer';

  getCustomers(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT
  ): Observable<PagedCustomerResult> {
    const params = new HttpParams()
      .set('skipCount', skipCount)
      .set('maxResultCount', maxResultCount);

    return this.http.get<PagedCustomerResult>(this.apiUrl, { params });
  }

  createCustomer(data: CreateUpdateCustomer): Observable<Customer> {
    return this.http.post<Customer>(this.apiUrl, data);
  }

  updateCustomer(
    id: string,
    data: CreateUpdateCustomer
  ): Observable<Customer> {
    return this.http.put<Customer>(`${this.apiUrl}/${id}`, data);
  }

  deleteCustomer(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
