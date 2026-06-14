import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { leaveCreateRoute } from 'src/app/core/utils/create-route.util';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import {
  CreateUpdateService,
  ServiceItem,
} from 'src/app/core/models/service.model';
import { ServiceService } from 'src/app/core/services/service.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import {
  AddServiceDialog,
  AddServiceDialogData,
} from 'src/app/shared/components/add-service-dialog/add-service-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { DEFAULT_PAGE_SIZE } from 'src/app/core/constants/pagination.constants';
import { getSkipCount } from 'src/app/core/utils/pagination.util';
import { DataTablePaginationComponent } from 'src/app/shared/components/data-table-pagination/data-table-pagination';

@Component({
  selector: 'app-services-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule, AppLocalizationPipe, DataTablePaginationComponent],
  templateUrl: './services-table.html',
  styleUrl: './services-table.scss',
})
export class ServicesTableComponent implements OnInit {
  private serviceService = inject(ServiceService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);

  services: ServiceItem[] = [];
  readonly pageIndex = signal(0);
  readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  readonly totalCount = signal(0);

  get canCreate(): boolean {
    return this.policy.hasSnapshot('BanquetHallManagement.Services.Create');
  }

  get canUpdate(): boolean {
    return this.policy.hasSnapshot('BanquetHallManagement.Services.Update');
  }

  get canDelete(): boolean {
    return this.policy.hasSnapshot('BanquetHallManagement.Services.Delete');
  }

  get showActionsColumn(): boolean {
    return this.canUpdate || this.canDelete;
  }

  ngOnInit(): void {
    this.loadServices();
  }

  loadServices(): void {
    const skip = getSkipCount(this.pageIndex(), this.pageSize());

    this.serviceService.getServices(skip, this.pageSize()).subscribe({
      next: (response) => {
        this.services = response.items;
        this.totalCount.set(response.totalCount);
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Services:LoadFailed'))
        );
      },
    });
  }

  onPageChange(pageIndex: number): void {
    this.pageIndex.set(pageIndex);
    this.loadServices();
  }

  openAddServiceDialog(
    initialData?: AddServiceDialogData
  ): void {
    const dialogRef = this.dialog.open(AddServiceDialog, {
      width: '700px',
      disableClose: true,
      data: initialData,
    });

    dialogRef.afterClosed().subscribe((result: CreateUpdateService | undefined) => {
      if (!result) {
        leaveCreateRoute(this.router, '/services/create', '/services');
        return;
      }

      this.serviceService.createService(result).subscribe({
        next: () => {
          this.loadServices();
          leaveCreateRoute(this.router, '/services/create', '/services');
        },
        error: (error) => {
          this.openAddServiceDialog({
            name: result.name,
            price: result.price,
            apiError: getAbpErrorMessage(error),
          });
        },
      });
    });
  }

  editService(id: string): void {
    const service = this.services.find((s) => s.id === id);

    if (!service) {
      return;
    }

    this.openEditServiceDialog(service);
  }

  private openEditServiceDialog(
    service: ServiceItem,
    apiError?: string
  ): void {
    const dialogRef = this.dialog.open(AddServiceDialog, {
      width: '700px',
      disableClose: true,
      data: {
        id: service.id,
        name: service.name,
        price: service.price,
        apiError,
      } satisfies AddServiceDialogData,
    });

    dialogRef.afterClosed().subscribe((result: CreateUpdateService | undefined) => {
      if (!result) {
        return;
      }

      this.serviceService.updateService(service.id, result).subscribe({
        next: () => {
          this.loadServices();
        },
        error: (error) => {
          this.openEditServiceDialog(
            {
              id: service.id,
              name: result.name,
              price: result.price,
            },
            getAbpErrorMessage(error)
          );
        },
      });
    });
  }

  deleteService(id: string): void {
    this.dialogService
      .confirm(
        this.l10n.instant('Services:Delete:Title'),
        this.l10n.instant('Services:Delete:Message')
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.serviceService.deleteService(id).subscribe({
          next: () => {
            this.loadServices();
          },
          error: (error) => {
            this.notification.showError(
              getAbpErrorMessage(error, this.l10n.instant('Services:DeleteFailed'))
            );
          },
        });
      });
  }
}
