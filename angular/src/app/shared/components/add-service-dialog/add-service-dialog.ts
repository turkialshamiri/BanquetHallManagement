import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import {
  CreateUpdateService,
  ServiceItem,
} from 'src/app/core/models/service.model';

interface ServiceFormErrors {
  name?: string;
  price?: string;
}

export interface AddServiceDialogData extends Partial<ServiceItem> {
  apiError?: string;
}

@Component({
  selector: 'app-add-service-dialog',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './add-service-dialog.html',
  styleUrl: './add-service-dialog.scss',
})
export class AddServiceDialog implements OnInit {
  dialogRef = inject(MatDialogRef<AddServiceDialog>);

  data = inject<AddServiceDialogData | undefined>(MAT_DIALOG_DATA, {
    optional: true,
  });

  isEditMode = false;

  service: CreateUpdateService = {
    name: '',
    price: 0,
  };

  errors: ServiceFormErrors = {};
  apiError = '';

  ngOnInit(): void {
    if (!this.data) {
      return;
    }

    if (this.data.id) {
      this.isEditMode = true;
    }

    this.service = {
      name: this.data.name ?? '',
      price: this.data.price ?? 0,
    };

    this.apiError = this.data.apiError ?? '';
  }

  save(): void {
    this.apiError = '';

    if (!this.validate()) {
      return;
    }

    const payload: CreateUpdateService = {
      name: this.service.name.trim(),
      price: this.service.price,
    };

    this.dialogRef.close(payload);
  }

  close(): void {
    this.dialogRef.close();
  }

  clearError(field: keyof ServiceFormErrors): void {
    if (this.errors[field]) {
      this.errors = { ...this.errors, [field]: undefined };
    }

    if (this.apiError) {
      this.apiError = '';
    }
  }

  private validate(): boolean {
    const errors: ServiceFormErrors = {};

    if (!this.service.name?.trim()) {
      errors.name = 'اسم الخدمة مطلوب';
    }

    if (this.service.price == null || this.service.price <= 0) {
      errors.price = 'سعر الخدمة يجب أن يكون أكبر من صفر';
    }

    this.errors = errors;
    return Object.keys(errors).length === 0;
  }
}
