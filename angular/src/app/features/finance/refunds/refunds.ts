import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { RefundService } from 'src/app/core/services/refund.service';
import { PendingRefund } from 'src/app/core/models/refund.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import {
  REFUND_DETAILS_DIALOG_CONFIG,
  RefundDetailsDialog,
} from 'src/app/shared/components/refund-details-dialog/refund-details-dialog';

@Component({
  selector: 'app-refunds',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, MatDialogModule, AppLocalizationPipe],
  templateUrl: './refunds.html',
  styleUrl: './refunds.scss',
})
export class Refunds implements OnInit {
  private refundService = inject(RefundService);
  private notification = inject(NotificationService);
  private dialogService = inject(DialogService);
  private dialog = inject(MatDialog);
  private l10n = inject(AppLocalizationService);
  private policy = inject(PolicyService);
  private statusL10n = inject(StatusLocalizationService);
  private router = inject(Router);

  readonly refunds = signal<PendingRefund[]>([]);
  readonly loading = signal(false);
  readonly processingId = signal<string | null>(null);
  readonly detailsLoadingId = signal<string | null>(null);
  readonly searchTerm = signal('');

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

  clearSearch(): void {
    if (!this.searchTerm()) {
      return;
    }

    this.searchTerm.set('');
    this.loadRefunds();
  }

  onSearchKeyup(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
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

  viewDetails(refund: PendingRefund): void {
    this.detailsLoadingId.set(refund.reservationId);

    this.refundService.getDetails(refund.reservationId).subscribe({
      next: (details) => {
        this.detailsLoadingId.set(null);
        this.dialog.open(RefundDetailsDialog, {
          ...REFUND_DETAILS_DIALOG_CONFIG,
          data: { details },
        });
      },
      error: (error) => {
        this.detailsLoadingId.set(null);
        this.notification.showError(getAbpErrorMessage(error));
      },
    });
  }

  refundStatusLabel(refund: PendingRefund): string {
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
