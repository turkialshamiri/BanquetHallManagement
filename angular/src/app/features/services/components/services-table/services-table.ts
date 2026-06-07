import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
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

@Component({
  selector: 'app-services-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule],
  templateUrl: './services-table.html',
  styleUrl: './services-table.scss',
})
export class ServicesTableComponent implements OnInit {
  private serviceService = inject(ServiceService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);

  services: ServiceItem[] = [];

  ngOnInit(): void {
    this.loadServices();
  }

  loadServices(): void {
    this.serviceService.getServices().subscribe({
      next: (response) => {
        this.services = response.items;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error(error);
      },
    });
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
        return;
      }

      this.serviceService.createService(result).subscribe({
        next: () => {
          this.loadServices();
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
        'حذف الخدمة',
        'هل أنت متأكد من حذف هذه الخدمة؟ لا يمكن التراجع عن العملية.'
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
            console.error(getAbpErrorMessage(error));
          },
        });
      });
  }
}
