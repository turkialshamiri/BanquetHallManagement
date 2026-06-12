import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { RefundService } from 'src/app/core/services/refund.service';
import { PendingRefund, RefundLiabilityLookup } from 'src/app/core/models/refund.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';

@Component({
  selector: 'app-refunds',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './refunds.html',
  styleUrl: './refunds.scss',
})
export class Refunds implements OnInit {
  private refundService = inject(RefundService);
  private notification = inject(NotificationService);
  private dialogService = inject(DialogService);
  private l10n = inject(AppLocalizationService);
  private policy = inject(PolicyService);
  private statusL10n = inject(StatusLocalizationService);
  private router = inject(Router);

  readonly refunds = signal<PendingRefund[]>([]);
  readonly loading = signal(false);
  readonly processingId = signal<string | null>(null);
  readonly searchTerm = signal('');
  readonly processReservationNumber = signal('');
  readonly lookup = signal<RefundLiabilityLookup | null>(null);
  readonly lookupLoading = signal(false);
  readonly processingByNumber = signal(false);

  readonly canProcess = this.policy.hasSnapshot(
    'BanquetHallManagement.Finance.Refunds.Process'
  );

  ngOnInit(): void {
    this.loadRefunds();
  }

  loadRefunds(): void {
    this.loading.set(true);

    this.refundService.getPending(this.searchTerm()).subscribe({
      next: (result) => {
        this.refunds.set(result.items);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Finance:Refunds:LoadFailed'))
        );
      },
    });
  }

  onSearch(): void {
    this.loadRefunds();
  }

  lookupRefund(): void {
    const reservationNumber = this.processReservationNumber().trim();
    if (!reservationNumber) {
      return;
    }

    this.lookupLoading.set(true);
    this.lookup.set(null);

    this.refundService.getByReservationNumber(reservationNumber).subscribe({
      next: (details) => {
        this.lookup.set(details);
        this.lookupLoading.set(false);
      },
      error: (error) => {
        this.lookupLoading.set(false);
        this.notification.showError(getAbpErrorMessage(error));
      },
    });
  }

  onLookupKeyup(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.lookupRefund();
    }
  }

  canProcessLookup(details: RefundLiabilityLookup): boolean {
    return details.statusCode === 'Pending' && details.liabilityAmount > 0;
  }

  processByNumber(): void {
    const details = this.lookup();
    if (!details || !this.canProcess || !this.canProcessLookup(details)) {
      return;
    }

    this.dialogService
      .confirm({
        type: 'confirm',
        title: this.l10n.instant('Finance:Refunds:Process:Title'),
        message: this.l10n.instant('Finance:Refunds:Process:Message'),
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.processingByNumber.set(true);

        this.refundService.processByReservationNumber(details.reservationNumber).subscribe({
          next: () => {
            this.processingByNumber.set(false);
            this.lookup.set(null);
            this.processReservationNumber.set('');
            this.notification.showSuccess(
              this.l10n.instant('Finance:Refunds:Process:Success')
            );
            this.loadRefunds();
          },
          error: (error) => {
            this.processingByNumber.set(false);
            this.notification.showError(getAbpErrorMessage(error));
          },
        });
      });
  }

  processRefund(refund: PendingRefund): void {
    if (!this.canProcess) {
      return;
    }

    this.dialogService
      .confirm({
        type: 'confirm',
        title: this.l10n.instant('Finance:Refunds:Process:Title'),
        message: this.l10n.instant('Finance:Refunds:Process:Message'),
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.processingId.set(refund.reservationId);

        this.refundService.process(refund.reservationId).subscribe({
          next: () => {
            this.processingId.set(null);
            this.notification.showSuccess(
              this.l10n.instant('Finance:Refunds:Process:Success')
            );
            this.loadRefunds();
          },
          error: (error) => {
            this.processingId.set(null);
            this.notification.showError(getAbpErrorMessage(error));
          },
        });
      });
  }

  refundStatusLabel(refund: PendingRefund | RefundLiabilityLookup): string {
    return this.statusL10n.refundStatus(refund.statusCode);
  }

  statusClass(statusCode: string): string {
    return `refund-status-${statusCode.toLowerCase()}`;
  }

  openJournalEntry(journalEntryId?: string | null): void {
    if (!journalEntryId) {
      return;
    }

    void this.router.navigate(['/finance/journal-entries', journalEntryId]);
  }
}
