import type { CreateUpdateReservationDto, ReservationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateReservationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationDto>({
      method: 'POST',
      url: '/api/app/reservation',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/reservation/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationDto>({
      method: 'GET',
      url: `/api/app/reservation/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReservationDto>>({
      method: 'GET',
      url: '/api/app/reservation',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateReservationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationDto>({
      method: 'PUT',
      url: `/api/app/reservation/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}