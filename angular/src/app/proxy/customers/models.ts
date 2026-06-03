
export interface CreateUpdateCustomerDto {
  name: string;
  phone: string;
  company?: string | null;
}

export interface CustomerDto {
  id?: string;
  name?: string;
  phone?: string;
  company?: string | null;
}
