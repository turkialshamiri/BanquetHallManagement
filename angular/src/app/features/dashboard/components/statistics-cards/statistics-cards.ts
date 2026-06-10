import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { DashboardStats } from 'src/app/core/models/dashboard.model';
import { DashboardService } from 'src/app/core/services/dashboard.service';
import { ConfigStateService } from '@abp/ng.core';
import { PolicyService } from 'src/app/core/services/policy.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';

@Component({
  selector: 'app-statistics-cards',
  standalone: true,
  imports: [CommonModule, DecimalPipe, AppLocalizationPipe],
  templateUrl: './statistics-cards.html',
  styleUrl: './statistics-cards.scss',
})
export class StatisticsCardsComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private policy = inject(PolicyService);
  private configState = inject(ConfigStateService);
  private cdr = inject(ChangeDetectorRef);
  private l10n = inject(AppLocalizationService);

  stats: DashboardStats | null = null;
  isLoading = true;
  loadError: string | null = null;

  get canViewRevenue(): boolean {
    return this.policy.hasSnapshot(
      'BanquetHallManagement.Dashboard.ViewRevenue'
    );
  }

  ngOnInit(): void {
    this.configState.getOne$('auth').subscribe(() => {
      this.cdr.markForCheck();
    });

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
          this.l10n.instant('Dashboard:LoadFailed')
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
