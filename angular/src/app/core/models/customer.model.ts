export interface Customer {
  id: string;
  name: string;
  phone: string;
  company?: string | null;
}

export interface CreateUpdateCustomer {
  name: string;
  phone: string;
  company?: string | null;
}

export interface PagedCustomerResult {
  items: Customer[];
  totalCount: number;
}
