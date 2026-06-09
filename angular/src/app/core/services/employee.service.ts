import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { DEFAULT_LIST_MAX_RESULT_COUNT } from '../constants/pagination.constants';
import {
  CreateEmployee,
  Employee,
  PagedEmployeeResult,
  UpdateEmployee,
} from '../models/employee.model';
import { normalizeEmployeeRoles } from '../utils/role-label.util';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:44324/api/app/employee';

  getEmployees(
    skipCount = 0,
    maxResultCount = DEFAULT_LIST_MAX_RESULT_COUNT
  ): Observable<PagedEmployeeResult> {
    const params = new HttpParams()
      .set('skipCount', skipCount)
      .set('maxResultCount', maxResultCount);

    return this.http
      .get<{ totalCount: number; items: Record<string, unknown>[] }>(this.apiUrl, {
        params,
      })
      .pipe(
        map((response) => ({
          totalCount: response.totalCount,
          items: (response.items ?? []).map((item) => this.mapEmployee(item)),
        }))
      );
  }

  createEmployee(data: CreateEmployee): Observable<Employee> {
    return this.http
      .post<Record<string, unknown>>(this.apiUrl, data)
      .pipe(map((item) => this.mapEmployee(item)));
  }

  updateEmployee(id: string, data: UpdateEmployee): Observable<Employee> {
    return this.http
      .put<Record<string, unknown>>(`${this.apiUrl}/${id}`, data)
      .pipe(map((item) => this.mapEmployee(item)));
  }

  deleteEmployee(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  activate(id: string): Observable<Employee> {
    return this.http
      .post<Record<string, unknown>>(`${this.apiUrl}/${id}/activate`, {})
      .pipe(map((item) => this.mapEmployee(item)));
  }

  deactivate(id: string): Observable<Employee> {
    return this.http
      .post<Record<string, unknown>>(`${this.apiUrl}/${id}/deactivate`, {})
      .pipe(map((item) => this.mapEmployee(item)));
  }

  resetPassword(id: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/reset-password`, {
      newPassword,
    });
  }

  private mapEmployee(raw: Record<string, unknown>): Employee {
    return {
      id: String(raw['id'] ?? ''),
      userName: String(raw['userName'] ?? raw['UserName'] ?? ''),
      email: String(raw['email'] ?? raw['Email'] ?? ''),
      name: String(raw['name'] ?? raw['Name'] ?? ''),
      surname: String(raw['surname'] ?? raw['Surname'] ?? ''),
      phoneNumber: String(raw['phoneNumber'] ?? raw['PhoneNumber'] ?? ''),
      isActive: Boolean(raw['isActive'] ?? raw['IsActive'] ?? false),
      roles: normalizeEmployeeRoles(raw['roles'] ?? raw['Roles']),
    };
  }
}

