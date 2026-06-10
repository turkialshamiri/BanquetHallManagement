import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { Hall } from 'src/app/core/models/hall.model';
import { HallService } from 'src/app/core/services/hall.service';
import { getHallStatusClass } from 'src/app/core/utils/hall-status.util';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import { AddHallDialog } from 'src/app/shared/components/add-hall-dialog/add-hall-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { PolicyService } from 'src/app/core/services/policy.service';

@Component({
  selector: 'app-halls-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule, AppLocalizationPipe],
  templateUrl: './halls-table.html',
  styleUrl: './halls-table.scss',
})
export class HallsTableComponent implements OnInit {
  private hallService = inject(HallService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  readonly statusL10n = inject(StatusLocalizationService);

  halls: Hall[] = [];
  pageTitleKey = 'Halls:Title';
  pageSubtitleKey = 'Halls:Subtitle';
  statusFilter: number | null = null;
  isLoading = false;
  loadError: string | null = null;

  getHallStatusClass = getHallStatusClass;

  canCreateHall = this.policy.hasSnapshot('BanquetHallManagement.Halls.Create');
  canUpdateHall = this.policy.hasSnapshot('BanquetHallManagement.Halls.Update');
  canDeleteHall = this.policy.hasSnapshot('BanquetHallManagement.Halls.Delete');

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.applyRouteConfig(data);
      this.loadHalls();
      this.cdr.markForCheck();
    });
  }

  loadHalls(): void {
    this.isLoading = true;
    this.loadError = null;

    this.hallService.getHallsList(this.statusFilter).subscribe({
      next: (halls) => {
        this.halls = halls;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.loadError = getAbpErrorMessage(
          error,
          this.l10n.instant('Halls:LoadFailed')
        );
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openAddHallDialog(): void {
    const dialogRef = this.dialog.open(AddHallDialog, {
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.hallService.createHall(result).subscribe({
        next: () => {
          this.loadHalls();
        },
        error: (error) => {
          this.notification.showError(
            getAbpErrorMessage(error, this.l10n.instant('Halls:CreateFailed'))
          );
        },
      });
    });
  }

  hallTypeLabel(type: number): string {
    return this.statusL10n.hallType(type);
  }

  hallStatusLabel(status: number): string {
    return this.statusL10n.hallStatus(status);
  }

  editHall(id: string): void {
    const hall = this.halls.find((h) => h.id === id);

    if (!hall) {
      return;
    }

    const dialogRef = this.dialog.open(AddHallDialog, {
      width: '700px',
      disableClose: true,
      data: hall,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.hallService.updateHall(id, result).subscribe({
        next: () => {
          this.loadHalls();
        },
        error: (error) => {
          this.notification.showError(
            getAbpErrorMessage(error, this.l10n.instant('Halls:UpdateFailed'))
          );
        },
      });
    });
  }

  deleteHall(id: string): void {
    this.dialogService
      .confirm(
        this.l10n.instant('Halls:Delete:Title'),
        this.l10n.instant('Halls:Delete:Message')
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.hallService.deleteHall(id).subscribe({
          next: () => {
            this.loadHalls();
          },
          error: (error) => {
            this.notification.showError(
              getAbpErrorMessage(error, this.l10n.instant('Halls:DeleteFailed'))
            );
          },
        });
      });
  }

  private applyRouteConfig(
    data: Record<string, unknown> = this.route.snapshot.data
  ): void {
    this.pageTitleKey = (data['pageTitleKey'] as string) ?? 'Halls:Title';
    this.pageSubtitleKey = (data['pageSubtitleKey'] as string) ?? 'Halls:Subtitle';
    this.statusFilter = (data['statusFilter'] as number | null) ?? null;
  }
}
