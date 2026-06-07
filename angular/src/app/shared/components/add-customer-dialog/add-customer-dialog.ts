import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { Customer, CreateUpdateCustomer } from 'src/app/core/models/customer.model';

interface CustomerFormErrors {
  name?: string;
  phone?: string;
}

@Component({
  selector: 'app-add-customer-dialog',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './add-customer-dialog.html',
  styleUrl: './add-customer-dialog.scss',
})
export class AddCustomerDialog implements OnInit {
  dialogRef = inject(MatDialogRef<AddCustomerDialog>);

  data = inject<Customer | undefined>(MAT_DIALOG_DATA, {
    optional: true,
  });

  isEditMode = false;

  customer: CreateUpdateCustomer = {
    name: '',
    phone: '',
    company: '',
  };

  errors: CustomerFormErrors = {};

  ngOnInit(): void {
    if (this.data) {
      this.isEditMode = true;
      this.customer = {
        name: this.data.name ?? '',
        phone: this.data.phone ?? '',
        company: this.data.company ?? '',
      };
    }
  }

  save(): void {
    if (!this.validate()) {
      return;
    }

    const payload: CreateUpdateCustomer = {
      name: this.customer.name.trim(),
      phone: this.customer.phone.trim(),
      company: this.customer.company?.trim() || null,
    };

    this.dialogRef.close(payload);
  }

  close(): void {
    this.dialogRef.close();
  }

  clearError(field: keyof CustomerFormErrors): void {
    if (this.errors[field]) {
      this.errors = { ...this.errors, [field]: undefined };
    }
  }

  private validate(): boolean {
    const errors: CustomerFormErrors = {};

    if (!this.customer.name?.trim()) {
      errors.name = 'اسم العميل مطلوب';
    }

    if (!this.customer.phone?.trim()) {
      errors.phone = 'رقم الهاتف مطلوب';
    }

    this.errors = errors;
    return Object.keys(errors).length === 0;
  }
}
