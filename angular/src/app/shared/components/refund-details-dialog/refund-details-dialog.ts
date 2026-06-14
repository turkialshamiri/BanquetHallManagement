import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { RefundDetails } from 'src/app/core/models/refund.model';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import { toTimeInputValue } from 'src/app/core/models/reservation.model';

export interface RefundDetailsDialogData {
  details: RefundDetails;
}

export const REFUND_DETAILS_DIALOG_CONFIG = {
  width: 'min(760px, 95vw)',
  maxWidth: '95vw',
  maxHeight: '90dvh',
  panelClass: 'refund-details-dialog-panel',
  disableClose: false,
} as const;

@Component({
  selector: 'app-refund-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    AppLocalizationPipe,
  ],
  templateUrl: './refund-details-dialog.html',
  styleUrl: './refund-details-dialog.scss',
})
export class RefundDetailsDialog {
  readonly data = inject<RefundDetailsDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<RefundDetailsDialog>);
  readonly statusL10n = inject(StatusLocalizationService);
  readonly formatTime = toTimeInputValue;

  close(): void {
    this.dialogRef.close();
  }

  reservationStatusLabel(status: string): string {
    return this.statusL10n.reservationStatus(status);
  }

  refundStatusLabel(statusCode: string): string {
    return this.statusL10n.refundStatus(statusCode);
  }
}
