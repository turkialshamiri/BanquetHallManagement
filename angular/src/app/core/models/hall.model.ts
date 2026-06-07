import { OperationalHallStatus } from '../utils/hall-status.util';

export interface Hall {
  id: string;
  name: string;
  description: string;
  capacity: number;
  location: string;
  pricePerHour: number;
  status: number;
  operationalStatus: number;
  type: number;
  creationTime: string;
}

export interface HallFormModel {
  name: string;
  description: string;
  capacity: number;
  location: string;
  pricePerHour: number;
  status: OperationalHallStatus;
  type: number;
}

export interface PagedHallResult {
  totalCount: number;
  items: Hall[];
}
