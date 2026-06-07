import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { Customer } from 'src/app/core/models/customer.model';
import { Hall } from 'src/app/core/models/hall.model';
import {
  CreateUpdateReservation,
  Reservation,
  toApiEventDate,
  toApiTimeSpan,
  toDateInputValue,
  toTimeInputValue,
} from 'src/app/core/models/reservation.model';
import { ServiceItem } from 'src/app/core/models/service.model';
import { CustomerService } from 'src/app/core/services/customer.service';
import { HallService } from 'src/app/core/services/hall.service';
import { ServiceService } from 'src/app/core/services/service.service';

interface ReservationFormErrors {
  customerId?: string;
  hallId?: string;
  eventDate?: string;
  startTime?: string;
  endTime?: string;
  guestsCount?: string;
}

export interface AddReservationDialogData extends Partial<Reservation> {
  apiError?: string;
}

interface ReservationFormState {
  customerId: string;
  hallId: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  guestsCount: number;
  serviceIds: string[];
}

@Component({
  selector: 'app-add-reservation-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-reservation-dialog.html',
  styleUrl: './add-reservation-dialog.scss',
})
export class AddReservationDialog implements OnInit {
  private customerService = inject(CustomerService);
  private hallService = inject(HallService);
  private serviceService = inject(ServiceService);
  private cdr = inject(ChangeDetectorRef);

  dialogRef = inject(MatDialogRef<AddReservationDialog>);

  data = inject<AddReservationDialogData | undefined>(MAT_DIALOG_DATA, {
    optional: true,
  });

  isEditMode = false;
  isLoading = true;

  customers: Customer[] = [];
  halls: Hall[] = [];
  services: ServiceItem[] = [];

  form: ReservationFormState = {
    customerId: '',
    hallId: '',
    eventDate: '',
    startTime: '',
    endTime: '',
    guestsCount: 0,
    serviceIds: [],
  };

  totalPrice: number | null = null;
  errors: ReservationFormErrors = {};
  apiError = '';

  ngOnInit(): void {
    if (this.data?.id) {
      this.isEditMode = true;
      this.totalPrice = this.data.totalPrice ?? null;

      this.form = {
        customerId: this.data.customerId ?? '',
        hallId: this.data.hallId ?? '',
        eventDate: toDateInputValue(this.data.eventDate ?? ''),
        startTime: toTimeInputValue(this.data.startTime ?? ''),
        endTime: toTimeInputValue(this.data.endTime ?? ''),
        guestsCount: this.data.guestsCount ?? 0,
        serviceIds: [...(this.data.serviceIds ?? [])],
      };
    }

    this.apiError = this.data?.apiError ?? '';

    this.loadDropdownData();
  }

  save(): void {
    this.apiError = '';

    if (!this.validate()) {
      return;
    }

    const payload: CreateUpdateReservation = {
      customerId: this.form.customerId,
      hallId: this.form.hallId,
      eventDate: toApiEventDate(this.form.eventDate),
      startTime: toApiTimeSpan(this.form.startTime),
      endTime: toApiTimeSpan(this.form.endTime),
      guestsCount: this.form.guestsCount,
      serviceIds: this.form.serviceIds.length
        ? this.form.serviceIds
        : [],
    };

    this.dialogRef.close(payload);
  }

  close(): void {
    this.dialogRef.close();
  }

  isServiceSelected(serviceId: string): boolean {
    return this.form.serviceIds.includes(serviceId);
  }

  toggleService(serviceId: string): void {
    if (this.isServiceSelected(serviceId)) {
      this.form.serviceIds = this.form.serviceIds.filter(
        (id) => id !== serviceId
      );
      return;
    }

    this.form.serviceIds = [...this.form.serviceIds, serviceId];
  }

  clearError(field: keyof ReservationFormErrors): void {
    if (this.errors[field]) {
      this.errors = { ...this.errors, [field]: undefined };
    }

    if (this.apiError) {
      this.apiError = '';
    }
  }

  private loadDropdownData(): void {
    forkJoin({
      customers: this.customerService.getCustomers(),
      halls: this.hallService.getHalls(),
      services: this.serviceService.getServices(),
    }).subscribe({
      next: ({ customers, halls, services }) => {
        this.customers = customers.items;
        this.halls = halls.items;
        this.services = services.items;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error(error);
        this.isLoading = false;
        this.apiError = 'تعذر تحميل بيانات النموذج';
        this.cdr.markForCheck();
      },
    });
  }

  private validate(): boolean {
    const errors: ReservationFormErrors = {};

    if (!this.form.customerId) {
      errors.customerId = 'يجب اختيار العميل';
    }

    if (!this.form.hallId) {
      errors.hallId = 'يجب اختيار القاعة';
    }

    if (!this.form.eventDate) {
      errors.eventDate = 'يجب تحديد تاريخ المناسبة';
    }

    if (!this.form.startTime) {
      errors.startTime = 'يجب تحديد وقت البداية';
    }

    if (!this.form.endTime) {
      errors.endTime = 'يجب تحديد وقت النهاية';
    }

    if (
      this.form.startTime &&
      this.form.endTime &&
      this.form.startTime >= this.form.endTime
    ) {
      errors.endTime = 'وقت البداية يجب أن يكون أقل من وقت النهاية';
    }

    if (!this.form.guestsCount || this.form.guestsCount <= 0) {
      errors.guestsCount = 'عدد الضيوف يجب أن يكون أكبر من صفر';
    }

    this.errors = errors;
    return Object.keys(errors).length === 0;
  }
}
