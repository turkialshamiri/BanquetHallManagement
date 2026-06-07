import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Customer } from 'src/app/core/models/customer.model';
import { CustomerService } from 'src/app/core/services/customer.service';
import { AddCustomerDialog } from 'src/app/shared/components/add-customer-dialog/add-customer-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';

@Component({
  selector: 'app-customers-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule],
  templateUrl: './customers-table.html',
  styleUrl: './customers-table.scss',
})
export class CustomersTableComponent implements OnInit {
  private customerService = inject(CustomerService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);

  customers: Customer[] = [];

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.customerService.getCustomers().subscribe({
      next: (response) => {
        this.customers = response.items;
      },
      error: (error) => {
        console.error(error);
      },
    });
  }

  openAddCustomerDialog(): void {
    const dialogRef = this.dialog.open(AddCustomerDialog, {
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.customerService.createCustomer(result).subscribe({
        next: () => {
          this.loadCustomers();
        },
        error: (error) => {
          console.error(error);
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
          console.error(error);
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
            console.error(error);
          },
        });
      });
  }
}
