import type { HallStatus } from '../enums/hall-status.enum';
import type { HallType } from '../enums/hall-type.enum';
import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateHallDto {
  name?: string;
  description?: string;
  location?: string;
  capacity?: number;
  pricePerHour?: number;
  status?: HallStatus;
  type?: HallType;
}

export interface HallDto extends AuditedEntityDto<string> {
  name?: string;
  description?: string;
  capacity?: number;
  location?: string;
  pricePerHour?: number;
  status?: HallStatus;
  type?: HallType;
}
