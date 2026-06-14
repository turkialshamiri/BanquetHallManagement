import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { toTimeInputValue } from 'src/app/core/models/reservation.model';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { ReservationService } from 'src/app/core/services/reservation.service';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { canArchiveReservation } from 'src/app/core/utils/reservation-status.util';
import { getRemainingAmount } from 'src/app/core/utils/payment.util';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import {
  ReservationDetailsDialogData,
} from './reservation-details-dialog.model';

@Component({
  selector: 'app-reservation-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    AppLocalizationPipe,
  ],
  templateUrl: './reservation-details-dialog.html',
  styleUrl: './reservation-details-dialog.scss',
})
export class ReservationDetailsDialog {
  readonly data = inject<ReservationDetailsDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<ReservationDetailsDialog>);
  readonly statusL10n = inject(StatusLocalizationService);
  readonly formatTime = toTimeInputValue;
  private readonly l10n = inject(AppLocalizationService);
  private readonly reservationService = inject(ReservationService);
  private readonly dialogService = inject(DialogService);
  private readonly notification = inject(NotificationService);

  readonly archiving = signal(false);

  close(): void {
    this.dialogRef.close();
  }

  showArchiveAction(): boolean {
    return (
      this.data.canArchive === true &&
      canArchiveReservation(this.data.details.reservation.status)
    );
  }

  confirmArchive(): void {
    this.dialogService
      .confirm({
        type: 'delete',
        title: this.l10n.instant('Reservations:Details:Archive:Title'),
        message: this.l10n.instant('Reservations:Details:Archive:Message'),
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.archiving.set(true);

        this.reservationService
          .archiveReservation(this.data.details.reservation.id)
          .subscribe({
            next: () => {
              this.archiving.set(false);
              this.dialogRef.close('archived');
            },
            error: (error) => {
              this.archiving.set(false);
              this.notification.showError(getAbpErrorMessage(error));
            },
          });
      });
  }

  reservationStatusLabel(status: string): string {
    return this.statusL10n.reservationStatus(status);
  }

  paymentStatusLabel(statusCode: string): string {
    return this.statusL10n.paymentStatus(statusCode);
  }

  hallTypeLabel(type?: number): string {
    if (type == null) {
      return '—';
    }

    return this.statusL10n.hallType(type);
  }

  remainingBalance(): number {
    return getRemainingAmount(this.data.details.reservation);
  }

  displayUserName(name?: string | null): string {
    if (name?.trim()) {
      return name.trim();
    }

    return this.l10n.instant('Reservations:Details:NotAvailable');
  }

  formatDateTime(value?: string | null): string {
    if (!value) {
      return '—';
    }

    return new Intl.DateTimeFormat(undefined, {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(value));
  }
}
