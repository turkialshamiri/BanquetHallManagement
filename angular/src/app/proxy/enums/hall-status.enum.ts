import { mapEnumToOptions } from '@abp/ng.core';

export enum HallStatus {
  Available = 1,
  Maintenance = 2,
  Booked = 3,
}

export const hallStatusOptions = mapEnumToOptions(HallStatus);
