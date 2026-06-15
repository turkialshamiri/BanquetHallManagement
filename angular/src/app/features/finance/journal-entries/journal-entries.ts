import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { FinanceLocalizationService } from 'src/app/core/services/finance-localization.service';
import { JournalEntryService } from 'src/app/core/services/journal-entry.service';
import { JournalEntry } from 'src/app/core/models/journal-entry.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { DEFAULT_PAGE_SIZE } from 'src/app/core/constants/pagination.constants';
import { getSkipCount } from 'src/app/core/utils/pagination.util';
import { DataTablePaginationComponent } from 'src/app/shared/components/data-table-pagination/data-table-pagination';

@Component({
  selector: 'app-journal-entries',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, AppLocalizationPipe, DataTablePaginationComponent],
  templateUrl: './journal-entries.html',
  styleUrl: './journal-entries.scss',
})
export class JournalEntries implements OnInit {
  private journalEntryService = inject(JournalEntryService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  private financeL10n = inject(FinanceLocalizationService);
  private router = inject(Router);

  readonly entries = signal<JournalEntry[]>([]);
  readonly loading = signal(false);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  readonly totalCount = signal(0);

  ngOnInit(): void {
    this.loadEntries();
  }

  loadEntries(): void {
    this.loading.set(true);
    const skip = getSkipCount(this.pageIndex(), this.pageSize());

    this.journalEntryService.getList(skip, this.pageSize()).subscribe({
      next: (result) => {
        this.entries.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(
            error,
            this.l10n.instant('Finance:JournalEntries:LoadFailed')
          )
        );
      },
    });
  }

  onPageChange(pageIndex: number): void {
    this.pageIndex.set(pageIndex);
    this.loadEntries();
  }

  viewEntry(id: string): void {
    void this.router.navigate(['/finance/journal-entries', id]);
  }

  sourceTypeLabel(sourceType: string): string {
    return this.financeL10n.journalSourceType(sourceType);
  }
}
