import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  getCustomers(): Observable<PagedCustomerResult> {
    return this.http.get<PagedCustomerResult>(this.apiUrl);
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
