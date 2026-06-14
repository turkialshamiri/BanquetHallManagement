import { HttpParams } from '@angular/common/http';
import {
  DEFAULT_PAGE_SIZE,
  DEFAULT_SORTING,
} from '../constants/pagination.constants';

export type PaginationToken = number | 'ellipsis';

export interface PaginationSlice<T> {
  items: T[];
  totalCount: number;
}

export function getSkipCount(pageIndex: number, pageSize = DEFAULT_PAGE_SIZE): number {
  return Math.max(0, pageIndex) * pageSize;
}

export function getTotalPages(totalCount: number, pageSize = DEFAULT_PAGE_SIZE): number {
  if (totalCount <= 0 || pageSize <= 0) {
    return 0;
  }

  return Math.ceil(totalCount / pageSize);
}

export function getRangeStart(
  pageIndex: number,
  totalCount: number,
  pageSize = DEFAULT_PAGE_SIZE
): number {
  if (totalCount <= 0) {
    return 0;
  }

  return pageIndex * pageSize + 1;
}

export function getRangeEnd(
  pageIndex: number,
  totalCount: number,
  pageSize = DEFAULT_PAGE_SIZE
): number {
  if (totalCount <= 0) {
    return 0;
  }

  return Math.min(totalCount, (pageIndex + 1) * pageSize);
}

export function getVisiblePages(
  pageIndex: number,
  totalPages: number
): PaginationToken[] {
  if (totalPages <= 0) {
    return [];
  }

  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index);
  }

  const pages = new Set<number>([0, totalPages - 1, pageIndex]);

  if (pageIndex > 0) {
    pages.add(pageIndex - 1);
  }

  if (pageIndex < totalPages - 1) {
    pages.add(pageIndex + 1);
  }

  if (pageIndex <= 2) {
    pages.add(1);
    pages.add(2);
    pages.add(3);
  }

  if (pageIndex >= totalPages - 3) {
    pages.add(totalPages - 2);
    pages.add(totalPages - 3);
    pages.add(totalPages - 4);
  }

  const sorted = [...pages].filter((page) => page >= 0 && page < totalPages).sort((a, b) => a - b);
  const tokens: PaginationToken[] = [];

  for (let index = 0; index < sorted.length; index++) {
    const page = sorted[index];
    const previous = sorted[index - 1];

    if (index > 0 && page - previous > 1) {
      tokens.push('ellipsis');
    }

    tokens.push(page);
  }

  return tokens;
}

export function sliceClientPage<T>(
  items: T[],
  pageIndex: number,
  pageSize = DEFAULT_PAGE_SIZE
): PaginationSlice<T> {
  const totalCount = items.length;
  const skip = getSkipCount(pageIndex, pageSize);

  return {
    totalCount,
    items: items.slice(skip, skip + pageSize),
  };
}

export function createPagingParams(
  skipCount: number,
  maxResultCount: number,
  sorting: string = DEFAULT_SORTING,
  extra?: Record<string, string | number | boolean | null | undefined>
): HttpParams {
  let params = new HttpParams()
    .set('skipCount', skipCount)
    .set('maxResultCount', maxResultCount)
    .set('sorting', sorting);

  if (!extra) {
    return params;
  }

  for (const [key, value] of Object.entries(extra)) {
    if (value !== undefined && value !== null && value !== '') {
      params = params.set(key, String(value));
    }
  }

  return params;
}
