import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { RefundService } from 'src/app/core/services/refund.service';
import { PendingRefund } from 'src/app/core/models/refund.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { toTimeInputValue } from 'src/app/core/models/reservation.model';

@Component({
  selector: 'app-refunds',
  standalone: true,
  imports: [CommonModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './refunds.html',
  styleUrl: './refunds.scss',
})
export class Refunds implements OnInit {
  private refundService = inject(RefundService);
  private notification = inject(NotificationService);
  private dialogService = inject(DialogService);
  private l10n = inject(AppLocalizationService);
  private policy = inject(PolicyService);

  readonly refunds = signal<PendingRefund[]>([]);
  readonly loading = signal(false);
  readonly processingId = signal<string | null>(null);
  readonly formatTime = toTimeInputValue;

  readonly canProcess = this.policy.hasSnapshot(
    'BanquetHallManagement.Finance.Refunds.Process'
  );

  ngOnInit(): void {
    this.loadRefunds();
  }

  loadRefunds(): void {
    this.loading.set(true);

    this.refundService.getPending().subscribe({
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
}
