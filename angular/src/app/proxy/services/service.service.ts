import type { CreateUpdateServiceDto, ServiceDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ServiceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateServiceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceDto>({
      method: 'POST',
      url: '/api/app/service',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/service/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceDto>({
      method: 'GET',
      url: `/api/app/service/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceDto>>({
      method: 'GET',
      url: '/api/app/service',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateServiceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceDto>({
      method: 'PUT',
      url: `/api/app/service/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}