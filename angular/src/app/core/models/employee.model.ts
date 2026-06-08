export interface Employee {
  id: string;
  userName: string;
  email: string;
  name: string;
  surname: string;
  phoneNumber: string;
  isActive: boolean;
  roles: string[];
}

export interface PagedEmployeeResult {
  items: Employee[];
  totalCount: number;
}

export interface CreateEmployee {
  userName: string;
  email: string;
  password: string;
  name: string;
  surname: string;
  phoneNumber: string;
  role: 'Admin' | 'Employee';
}

export interface UpdateEmployee {
  email: string;
  name: string;
  surname: string;
  phoneNumber: string;
  isActive: boolean;
  role: 'Admin' | 'Employee';
}

