export interface ServiceItem {
  id: string;
  name: string;
  price: number;
}

export interface CreateUpdateService {
  name: string;
  price: number;
}

export interface PagedServiceResult {
  items: ServiceItem[];
  totalCount: number;
}
