import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { InvoiceService } from 'src/app/core/services/invoice.service';
import { Invoice } from 'src/app/core/models/invoice.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { DEFAULT_PAGE_SIZE } from 'src/app/core/constants/pagination.constants';
import { getSkipCount } from 'src/app/core/utils/pagination.util';
import { DataTablePaginationComponent } from 'src/app/shared/components/data-table-pagination/data-table-pagination';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, AppLocalizationPipe, DataTablePaginationComponent],
  templateUrl: './invoices.html',
  styleUrl: './invoices.scss',
})
export class Invoices implements OnInit {
  private invoiceService = inject(InvoiceService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  private router = inject(Router);

  readonly invoices = signal<Invoice[]>([]);
  readonly loading = signal(false);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  readonly totalCount = signal(0);

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.loading.set(true);
    const skip = getSkipCount(this.pageIndex(), this.pageSize());

    this.invoiceService.getList(skip, this.pageSize()).subscribe({
      next: (result) => {
        this.invoices.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Finance:Invoices:LoadFailed'))
        );
      },
    });
  }

  onPageChange(pageIndex: number): void {
    this.pageIndex.set(pageIndex);
    this.loadInvoices();
  }

  viewInvoice(id: string): void {
    void this.router.navigate(['/finance/invoices', id]);
  }

  invoiceTypeLabel(type: string): string {
    const key = `Enum:InvoiceType:${type}`;
    const translated = this.l10n.instant(key);
    return translated === key ? type : translated;
  }
}
