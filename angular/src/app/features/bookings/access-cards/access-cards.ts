import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { HttpErrorResponse } from '@angular/common/http';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { HallAccessCardService } from 'src/app/core/services/hall-access-card.service';
import { ReservationService } from 'src/app/core/services/reservation.service';
import {
  HallAccessCardEntryPreview,
  HallAccessCardListItem,
} from 'src/app/core/models/hall-access-card.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import { toTimeInputValue } from 'src/app/core/models/reservation.model';
import { RESERVATION_STATUS } from 'src/app/core/utils/reservation-status.util';
import { DEFAULT_PAGE_SIZE } from 'src/app/core/constants/pagination.constants';
import { getSkipCount } from 'src/app/core/utils/pagination.util';
import { DataTablePaginationComponent } from 'src/app/shared/components/data-table-pagination/data-table-pagination';

@Component({
  selector: 'app-access-cards',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatIconModule,
    AppLocalizationPipe,
    DataTablePaginationComponent,
  ],
  templateUrl: './access-cards.html',
  styleUrl: './access-cards.scss',
})
export class AccessCards implements OnInit {
  private accessCardService = inject(HallAccessCardService);
  private reservationService = inject(ReservationService);
  private router = inject(Router);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  private policy = inject(PolicyService);
  private statusL10n = inject(StatusLocalizationService);

  readonly cards = signal<HallAccessCardListItem[]>([]);
  readonly loading = signal(false);
  readonly confirming = signal(false);
  readonly loadingPreview = signal(false);
  readonly searchTerm = signal('');
  readonly pageIndex = signal(0);
  readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  readonly totalCount = signal(0);
  readonly entryPreview = signal<HallAccessCardEntryPreview | null>(null);
  readonly confirmationSucceeded = signal(false);
  readonly formatTime = toTimeInputValue;

  readonly canConfirmEntry = this.policy.hasSnapshot(
    'BanquetHallManagement.Reservations.ConfirmHallEntry'
  );

  readonly canPrint = this.policy.hasSnapshot(
    'BanquetHallManagement.Finance.HallAccessCards.Print'
  );

  ngOnInit(): void {
    this.loadCards();
  }

  onSearch(): void {
    this.pageIndex.set(0);
    this.confirmationSucceeded.set(false);
    this.loadCards();
    this.loadEntryPreview();
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.pageIndex.set(0);
    this.entryPreview.set(null);
    this.confirmationSucceeded.set(false);
    this.loadCards();
  }

  loadCards(): void {
    this.loading.set(true);
    const skip = getSkipCount(this.pageIndex(), this.pageSize());

    this.accessCardService.getList(skip, this.pageSize(), this.searchTerm()).subscribe({
      next: (result) => {
        this.cards.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(
            error,
            this.l10n.instant('Finance:AccessCards:LoadFailed')
          )
        );
      },
    });
  }

  onPageChange(pageIndex: number): void {
    this.pageIndex.set(pageIndex);
    this.loadCards();
  }

  openPrint(cardId: string): void {
    void this.router.navigate(['/finance/access-cards', cardId]);
  }

  loadEntryPreview(): void {
    const search = this.searchTerm().trim();
    if (!search) {
      this.entryPreview.set(null);
      return;
    }

    this.loadingPreview.set(true);
    this.entryPreview.set(null);

    this.accessCardService.getEntryPreviewBySearch(search).subscribe({
      next: (preview) => {
        this.entryPreview.set(preview);
        this.loadingPreview.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadingPreview.set(false);
        if (error.status === 404 || this.isNotFoundError(error)) {
          return;
        }

        this.notification.showError(
          getAbpErrorMessage(
            error,
            this.l10n.instant('Finance:AccessCards:ConfirmEntry:LoadFailed')
          )
        );
      },
    });
  }

  confirmEntry(): void {
    const preview = this.entryPreview();
    if (!preview?.canConfirmEntry || this.confirming() || !this.canConfirmEntry) {
      return;
    }

    this.confirming.set(true);
    this.confirmationSucceeded.set(false);

    this.reservationService
      .confirmHallEntry(preview.reservationId)
      .subscribe({
        next: () => {
          this.confirming.set(false);
          this.confirmationSucceeded.set(true);
          this.notification.showSuccess(
            this.l10n.instant('Finance:AccessCards:ConfirmEntry:Success')
          );
          this.loadCards();
          this.loadEntryPreview();
        },
        error: (error) => {
          this.confirming.set(false);
          this.notification.showError(getAbpErrorMessage(error));
        },
      });
  }

  paymentStatusLabel(status: string): string {
    return this.statusL10n.paymentStatus(status);
  }

  reservationStatusLabel(status: string): string {
    return this.statusL10n.reservationStatus(status);
  }

  isCompleted(status: string): boolean {
    return status === RESERVATION_STATUS.Completed;
  }

  qrCodeUrl(card: HallAccessCardListItem | HallAccessCardEntryPreview): string {
    const payload = `${card.reservationNumber}|${card.cardNumber}`;
    return `https://api.qrserver.com/v1/create-qr-code/?size=140x140&data=${encodeURIComponent(payload)}`;
  }

  private isNotFoundError(error: HttpErrorResponse): boolean {
    const message = getAbpErrorMessage(error, '');
    return (
      message.includes('HallAccessCard:NotFound') ||
      message.includes('Hall access card not found') ||
      message.includes('بطاقة الدخول')
    );
  }
}
