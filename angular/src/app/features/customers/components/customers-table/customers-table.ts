import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { leaveCreateRoute } from 'src/app/core/utils/create-route.util';
import { Customer } from 'src/app/core/models/customer.model';
import { CustomerService } from 'src/app/core/services/customer.service';
import { AddCustomerDialog } from 'src/app/shared/components/add-customer-dialog/add-customer-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';

@Component({
  selector: 'app-customers-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule],
  templateUrl: './customers-table.html',
  styleUrl: './customers-table.scss',
})
export class CustomersTableComponent implements OnInit {
  private customerService = inject(CustomerService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);
  private notification = inject(NotificationService);

  customers: Customer[] = [];
  canCreate = this.policy.hasSnapshot('BanquetHallManagement.Customers.Create');
  canUpdate = this.policy.hasSnapshot('BanquetHallManagement.Customers.Update');
  canDelete = this.policy.hasSnapshot('BanquetHallManagement.Customers.Delete');

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.customerService.getCustomers().subscribe({
      next: (response) => {
        this.customers = response.items;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.notification.showError(
          getAbpErrorMessage(error, 'تعذّر تحميل العملاء')
        );
      },
    });
  }

  openAddCustomerDialog(): void {
    const dialogRef = this.dialog.open(AddCustomerDialog, {
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        leaveCreateRoute(this.router, '/customers/create', '/customers');
        return;
      }

      this.customerService.createCustomer(result).subscribe({
        next: () => {
          this.loadCustomers();
          leaveCreateRoute(this.router, '/customers/create', '/customers');
        },
        error: (error) => {
          this.notification.showError(
            getAbpErrorMessage(error, 'تعذّر إضافة العميل')
          );
        },
      });
    });
  }

  editCustomer(id: string): void {
    const customer = this.customers.find((c) => c.id === id);

    if (!customer) {
      return;
    }

    const dialogRef = this.dialog.open(AddCustomerDialog, {
      width: '700px',
      disableClose: true,
      data: customer,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.customerService.updateCustomer(id, result).subscribe({
        next: () => {
          this.loadCustomers();
        },
        error: (error) => {
          this.notification.showError(
            getAbpErrorMessage(error, 'تعذّر تحديث العميل')
          );
        },
      });
    });
  }

  deleteCustomer(id: string): void {
    this.dialogService
      .confirm(
        'حذف العميل',
        'هل أنت متأكد من حذف هذا العميل؟ لا يمكن التراجع عن العملية.'
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.customerService.deleteCustomer(id).subscribe({
          next: () => {
            this.loadCustomers();
          },
          error: (error) => {
            this.notification.showError(
              getAbpErrorMessage(error, 'تعذّر حذف العميل')
            );
          },
        });
      });
  }
}
