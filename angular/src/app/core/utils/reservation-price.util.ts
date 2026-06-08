import { Hall } from '../models/hall.model';
import { ServiceItem } from '../models/service.model';

export function calculateReservationHours(
  startTime: string,
  endTime: string
): number {
  if (!startTime || !endTime) {
    return 0;
  }

  const [startHour, startMinute] = startTime.split(':').map(Number);
  const [endHour, endMinute] = endTime.split(':').map(Number);

  if (
    Number.isNaN(startHour) ||
    Number.isNaN(startMinute) ||
    Number.isNaN(endHour) ||
    Number.isNaN(endMinute)
  ) {
    return 0;
  }

  const startTotalMinutes = startHour * 60 + startMinute;
  const endTotalMinutes = endHour * 60 + endMinute;
  const diffMinutes = endTotalMinutes - startTotalMinutes;

  return diffMinutes > 0 ? diffMinutes / 60 : 0;
}

export interface ReservationPriceBreakdown {
  hallName: string;
  pricePerHour: number;
  hours: number;
  hallCost: number;
  selectedServices: Array<{ name: string; price: number }>;
  servicesCost: number;
  totalCost: number;
  isValid: boolean;
}

export function calculateReservationPrice(
  hall: Hall | undefined,
  services: ServiceItem[],
  selectedServiceIds: string[],
  startTime: string,
  endTime: string
): ReservationPriceBreakdown {
  const hours = calculateReservationHours(startTime, endTime);
  const pricePerHour = hall?.pricePerHour ?? 0;
  const hallCost = hours > 0 ? hours * pricePerHour : 0;

  const selectedServices = services
    .filter((service) => selectedServiceIds.includes(service.id))
    .map((service) => ({ name: service.name, price: service.price }));

  const servicesCost = selectedServices.reduce(
    (sum, service) => sum + service.price,
    0
  );

  const totalCost = hallCost + servicesCost;

  return {
    hallName: hall?.name ?? '—',
    pricePerHour,
    hours,
    hallCost,
    selectedServices,
    servicesCost,
    totalCost,
    isValid: hours > 0,
  };
}
