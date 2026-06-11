import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { Reservation } from 'src/app/core/models/reservation.model';
import {
  calculateMinimumDeposit,
  calculateMinimumInstallment,
  isDepositPayment,
} from 'src/app/core/utils/payment.util';

export interface RecordPaymentDialogData {
  reservation: Reservation;
  customerName: string;
  hallName: string;
  apiError?: string;
}

export interface RecordPaymentDialogResult {
  amount: number;
  isDeposit: boolean;
}

export const RECORD_PAYMENT_DIALOG_CONFIG = {
  width: 'min(560px, 95vw)',
  maxWidth: '95vw',
  maxHeight: '90dvh',
  panelClass: 'record-payment-dialog-panel',
  disableClose: true,
} as const;

@Component({
  selector: 'app-record-payment-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, MatButtonModule, AppLocalizationPipe],
  templateUrl: './record-payment-dialog.html',
  styleUrl: './record-payment-dialog.scss',
})
export class RecordPaymentDialog {
  private l10n = inject(AppLocalizationService);
  dialogRef = inject(MatDialogRef<RecordPaymentDialog>);
  data = inject<RecordPaymentDialogData>(MAT_DIALOG_DATA);

  readonly amount = signal(this.suggestInitialAmount());
  readonly amountError = signal<string | null>(null);

  readonly isDeposit = computed(() =>
    isDepositPayment(this.data.reservation.status, this.data.reservation.paidAmount ?? 0)
  );

  readonly remainingAmount = computed(() => {
    const total = this.data.reservation.totalPrice;
    const paid = this.data.reservation.paidAmount ?? 0;
    return Math.max(0, total - paid);
  });

  readonly minimumAmount = computed(() => {
    const total = this.data.reservation.totalPrice;
    const remaining = this.remainingAmount();

    if (this.isDeposit()) {
      return calculateMinimumDeposit(total);
    }

    const minInstallment = calculateMinimumInstallment(total);
    return remaining <= minInstallment ? remaining : minInstallment;
  });

  get apiError(): string | undefined {
    return this.data.apiError;
  }

  close(): void {
    this.dialogRef.close();
  }

  submit(): void {
    const value = this.amount();
    const min = this.minimumAmount();
    const max = this.remainingAmount();

    if (!value || value <= 0) {
      this.amountError.set(this.l10n.instant('Finance:Payment:Validation:AmountRequired'));
      return;
    }

    if (value < min - 0.001) {
      this.amountError.set(
        this.l10n.instant('Finance:Payment:Validation:BelowMinimum', String(min))
      );
      return;
    }

    if (value > max + 0.001) {
      this.amountError.set(
        this.l10n.instant('Finance:Payment:Validation:AboveRemaining', String(max))
      );
      return;
    }

    this.dialogRef.close({
      amount: value,
      isDeposit: this.isDeposit(),
    } satisfies RecordPaymentDialogResult);
  }

  updateAmount(raw: string): void {
    const parsed = Number(raw);
    this.amount.set(Number.isFinite(parsed) ? parsed : 0);
    this.amountError.set(null);
  }

  private suggestInitialAmount(): number {
    const remaining = Math.max(
      0,
      this.data.reservation.totalPrice - (this.data.reservation.paidAmount ?? 0)
    );

    if (isDepositPayment(this.data.reservation.status, this.data.reservation.paidAmount ?? 0)) {
      return calculateMinimumDeposit(this.data.reservation.totalPrice);
    }

    const minInstallment = calculateMinimumInstallment(this.data.reservation.totalPrice);
    return remaining <= minInstallment ? remaining : minInstallment;
  }
}
