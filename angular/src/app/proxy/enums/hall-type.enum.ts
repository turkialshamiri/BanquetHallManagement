import { mapEnumToOptions } from '@abp/ng.core';

export enum HallType {
  Wedding = 1,
  Conference = 2,
  Meeting = 3,
}

export const hallTypeOptions = mapEnumToOptions(HallType);
