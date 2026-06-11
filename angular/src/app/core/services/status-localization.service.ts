import { Injectable, inject } from '@angular/core';
import { AppLocalizationService } from './app-localization.service';
import { HALL_STATUS } from '../utils/hall-status.util';
import { RESERVATION_STATUS } from '../utils/reservation-status.util';

@Injectable({ providedIn: 'root' })
export class StatusLocalizationService {
  private readonly l10n = inject(AppLocalizationService);

  hallStatus(status: number): string {
    const keyByStatus: Record<number, string> = {
      [HALL_STATUS.Available]: 'Enum:HallStatus:Available',
      [HALL_STATUS.Maintenance]: 'Enum:HallStatus:Maintenance',
      [HALL_STATUS.Booked]: 'Enum:HallStatus:Booked',
    };

    return this.l10n.instant(keyByStatus[status] ?? 'Unknown');
  }

  hallType(type: number): string {
    const keyByType: Record<number, string> = {
      1: 'Enum:HallType:Wedding',
      2: 'Enum:HallType:Conference',
      3: 'Enum:HallType:Meeting',
    };

    return this.l10n.instant(keyByType[type] ?? 'Unknown');
  }

  reservationStatus(status: string): string {
    const keyByStatus: Record<string, string> = {
      [RESERVATION_STATUS.Pending]: 'Enum:ReservationStatus:Pending',
      [RESERVATION_STATUS.Confirmed]: 'Enum:ReservationStatus:Confirmed',
      [RESERVATION_STATUS.Cancelled]: 'Enum:ReservationStatus:Cancelled',
      [RESERVATION_STATUS.Completed]: 'Enum:ReservationStatus:Completed',
      [RESERVATION_STATUS.FullyPaid]: 'Enum:ReservationStatus:FullyPaid',
    };

    if (!status) {
      return '—';
    }

    return this.l10n.instant(keyByStatus[status] ?? status);
  }

  roleLabel(role: string): string {
    const normalized = role?.toLowerCase();
    if (normalized === 'admin') {
      return this.l10n.instant('Enum:Role:Admin');
    }
    if (normalized === 'employee') {
      return this.l10n.instant('Enum:Role:Employee');
    }
    return role;
  }
}
