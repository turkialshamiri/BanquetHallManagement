import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { InvoiceService } from 'src/app/core/services/invoice.service';
import { InvoicePrintData } from 'src/app/core/models/invoice.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import { toTimeInputValue } from 'src/app/core/models/reservation.model';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './invoice-detail.html',
  styleUrl: './invoice-detail.scss',
})
export class InvoiceDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private invoiceService = inject(InvoiceService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  readonly statusL10n = inject(StatusLocalizationService);

  readonly invoice = signal<InvoicePrintData | null>(null);
  readonly loading = signal(true);
  readonly formatTime = toTimeInputValue;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }

    this.invoiceService.getPrintData(id).subscribe({
      next: (data) => {
        this.invoice.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Finance:Invoices:Detail:LoadFailed'))
        );
      },
    });
  }

  print(): void {
    window.print();
  }

  invoiceTypeLabel(type: string): string {
    const key = `Enum:InvoiceType:${type}`;
    const translated = this.l10n.instant(key);
    return translated === key ? type : translated;
  }
}
