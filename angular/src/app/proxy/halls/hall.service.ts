import type { CreateUpdateHallDto, HallDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { HallStatus } from '../enums/hall-status.enum';

@Injectable({
  providedIn: 'root',
})
export class HallService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateHallDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HallDto>({
      method: 'POST',
      url: '/api/app/hall',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/hall/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HallDto>({
      method: 'GET',
      url: `/api/app/hall/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByStatus = (status: HallStatus, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HallDto[]>({
      method: 'GET',
      url: '/api/app/hall/by-status',
      params: { status },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<HallDto>>({
      method: 'GET',
      url: '/api/app/hall',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateHallDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HallDto>({
      method: 'PUT',
      url: `/api/app/hall/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}