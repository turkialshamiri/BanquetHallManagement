import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { Hall } from 'src/app/core/models/hall.model';
import {
  REPORT_STATUS_OPTIONS,
  ReportFilters,
  ReportsResult,
} from 'src/app/core/models/report.model';
import { HallService } from 'src/app/core/services/hall.service';
import { ReportService } from 'src/app/core/services/report.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DecimalPipe,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports implements OnInit {
  private reportService = inject(ReportService);
  private hallService = inject(HallService);
  private cdr = inject(ChangeDetectorRef);

  reports: ReportsResult | null = null;
  halls: Hall[] = [];
  isLoading = false;
  loadError: string | null = null;

  readonly statusOptions = REPORT_STATUS_OPTIONS;

  filters: ReportFilters = this.createDefaultFilters();
  appliedFilters: ReportFilters = this.createDefaultFilters();

  hallColumns = [
    'hallName',
    'reservationCount',
    'revenue',
    'averageGuests',
    'occupancyRate',
  ];

  customerColumns = ['customerName', 'reservationCount', 'totalSpent'];
  monthlyColumns = ['monthLabel', 'reservationCount', 'revenue'];

  ngOnInit(): void {
    forkJoin({
      halls: this.hallService.getHalls(),
    }).subscribe({
      next: ({ halls }) => {
        this.halls = halls.items;
        this.loadReports();
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadReports();
        this.cdr.markForCheck();
      },
    });
  }

  applyFilters(): void {
    this.appliedFilters = { ...this.filters };
    this.loadReports();
  }

  resetFilters(): void {
    this.filters = this.createDefaultFilters();
    this.appliedFilters = { ...this.filters };
    this.loadReports();
  }

  loadReports(): void {
    this.isLoading = true;
    this.loadError = null;

    this.reportService.getReports(this.appliedFilters).subscribe({
      next: (reports) => {
        this.reports = reports;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.loadError = getAbpErrorMessage(
          error,
          'تعذّر تحميل التقارير'
        );
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  formatTrend(changePercent: number | null | undefined): string {
    if (changePercent == null) {
      return '—';
    }

    const prefix = changePercent > 0 ? '+' : '';

    return `${prefix}${changePercent}%`;
  }

  trendClass(changePercent: number | null | undefined): string {
    if (changePercent == null) {
      return 'neutral';
    }

    if (changePercent > 0) {
      return 'positive';
    }

    if (changePercent < 0) {
      return 'negative';
    }

    return 'neutral';
  }

  private createDefaultFilters(): ReportFilters {
    const today = new Date();
    const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);

    return {
      dateFrom: this.toDateInputValue(monthStart),
      dateTo: this.toDateInputValue(today),
      hallId: null,
      status: null,
    };
  }

  private toDateInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
