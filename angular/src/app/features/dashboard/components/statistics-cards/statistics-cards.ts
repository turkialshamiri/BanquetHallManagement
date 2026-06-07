import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { DashboardStats } from 'src/app/core/models/dashboard.model';
import { DashboardService } from 'src/app/core/services/dashboard.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';


@Component({
  selector: 'app-statistics-cards',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  templateUrl: './statistics-cards.html',
  styleUrl: './statistics-cards.scss',
})
export class StatisticsCardsComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);

  stats: DashboardStats | null = null;
  isLoading = true;
  loadError: string | null = null;

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading = true;
    this.loadError = null;

    this.dashboardService.getStats().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.loadError = getAbpErrorMessage(
          error,
          'تعذّر تحميل إحصائيات لوحة القيادة'
        );
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  get isEmpty(): boolean {
    if (!this.stats) {
      return false;
    }

    return (
      this.stats.totalHalls === 0 &&
      this.stats.totalCustomers === 0 &&
      this.stats.totalServices === 0 &&
      this.stats.totalReservations === 0
    );
  }
}
