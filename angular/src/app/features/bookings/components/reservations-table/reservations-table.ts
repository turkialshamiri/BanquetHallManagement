import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { Customer } from 'src/app/core/models/customer.model';
import { Hall } from 'src/app/core/models/hall.model';
import {
  CreateUpdateReservation,
  Reservation,
  toTimeInputValue,
} from 'src/app/core/models/reservation.model';
import { ReservationService } from 'src/app/core/services/reservation.service';
import { CustomerService } from 'src/app/core/services/customer.service';
import { HallService } from 'src/app/core/services/hall.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import {
  canCancelReservation,
  canCompleteReservation,
  canConfirmReservation,
  canDeleteReservation,
  canEditReservation,
  getReservationStatusClass,
  getReservationStatusLabel,
} from 'src/app/core/utils/reservation-status.util';
import {
  AddReservationDialog,
  AddReservationDialogData,
} from 'src/app/shared/components/add-reservation-dialog/add-reservation-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { PolicyService } from 'src/app/core/services/policy.service';

@Component({
  selector: 'app-reservations-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule],
  templateUrl: './reservations-table.html',
  styleUrl: './reservations-table.scss',
})
export class ReservationsTableComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private customerService = inject(CustomerService);
  private hallService = inject(HallService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);

  reservations: Reservation[] = [];
  customersMap = new Map<string, Customer>();
  hallsMap = new Map<string, Hall>();

  getReservationStatusLabel = getReservationStatusLabel;
  getReservationStatusClass = getReservationStatusClass;
  canConfirmReservation = canConfirmReservation;
  canCancelReservation = canCancelReservation;
  canCompleteReservation = canCompleteReservation;
  canEditReservation = canEditReservation;
  canDeleteReservation = canDeleteReservation;
  formatTime = toTimeInputValue;

  canCreate = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Create');
  canUpdate = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Update');
  canDelete = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Delete');
  canConfirm = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Confirm');
  canCancel = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Cancel');
  canComplete = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Complete');

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    forkJoin({
      reservations: this.reservationService.getReservations(),
      customers: this.customerService.getCustomers(),
      halls: this.hallService.getHalls(),
    }).subscribe({
      next: ({ reservations, customers, halls }) => {
        this.reservations = reservations.items;
        this.customersMap = new Map(
          customers.items.map((customer) => [customer.id, customer])
        );
        this.hallsMap = new Map(
          halls.items.map((hall) => [hall.id, hall])
        );
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error(error);
      },
    });
  }

  getCustomerName(customerId: string): string {
    return this.customersMap.get(customerId)?.name ?? '—';
  }

  getHallName(hallId: string): string {
    return this.hallsMap.get(hallId)?.name ?? '—';
  }

  openAddReservationDialog(
    initialData?: AddReservationDialogData
  ): void {
    const dialogRef = this.dialog.open(AddReservationDialog, {
      width: '760px',
      disableClose: true,
      data: initialData,
    });

    dialogRef
      .afterClosed()
      .subscribe((result: CreateUpdateReservation | undefined) => {
        if (!result) {
          return;
        }

        this.reservationService.createReservation(result).subscribe({
          next: () => {
            this.loadData();
          },
          error: (error) => {
            this.openAddReservationDialog({
              customerId: result.customerId,
              hallId: result.hallId,
              eventDate: result.eventDate,
              startTime: result.startTime,
              endTime: result.endTime,
              guestsCount: result.guestsCount,
              serviceIds: result.serviceIds ?? [],
              apiError: getAbpErrorMessage(error),
            });
          },
        });
      });
  }

  editReservation(id: string): void {
    const reservation = this.reservations.find((item) => item.id === id);

    if (!reservation || !canEditReservation(reservation.status)) {
      return;
    }

    this.openEditReservationDialog(reservation);
  }

  confirmReservation(id: string): void {
    this.dialogService
      .confirm({
        type: 'confirm',
        title: 'تأكيد الحجز',
        message: 'هل أنت متأكد من تأكيد هذا الحجز؟',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.reservationService.confirmReservation(id).subscribe({
          next: () => {
            this.loadData();
          },
          error: (error) => {
            console.error(getAbpErrorMessage(error));
          },
        });
      });
  }

  cancelReservation(id: string): void {
    this.dialogService
      .confirm({
        type: 'cancel',
        title: 'إلغاء الحجز',
        message: 'هل أنت متأكد من إلغاء هذا الحجز؟',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.reservationService.cancelReservation(id).subscribe({
          next: () => {
            this.loadData();
          },
          error: (error) => {
            console.error(getAbpErrorMessage(error));
          },
        });
      });
  }

  completeReservation(id: string): void {
    this.dialogService
      .confirm({
        type: 'complete',
        title: 'إكمال الحجز',
        message: 'هل أنت متأكد من إكمال هذا الحجز؟',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.reservationService.completeReservation(id).subscribe({
          next: () => {
            this.loadData();
          },
          error: (error) => {
            console.error(getAbpErrorMessage(error));
          },
        });
      });
  }

  private openEditReservationDialog(
    reservation: Reservation,
    apiError?: string
  ): void {
    const dialogRef = this.dialog.open(AddReservationDialog, {
      width: '760px',
      disableClose: true,
      data: {
        id: reservation.id,
        customerId: reservation.customerId,
        hallId: reservation.hallId,
        eventDate: reservation.eventDate,
        startTime: reservation.startTime,
        endTime: reservation.endTime,
        guestsCount: reservation.guestsCount,
        serviceIds: reservation.serviceIds ?? [],
        totalPrice: reservation.totalPrice,
        status: reservation.status,
        apiError,
      } satisfies AddReservationDialogData,
    });

    dialogRef
      .afterClosed()
      .subscribe((result: CreateUpdateReservation | undefined) => {
        if (!result) {
          return;
        }

        this.reservationService
          .updateReservation(reservation.id, result)
          .subscribe({
            next: () => {
              this.loadData();
            },
            error: (error) => {
              this.openEditReservationDialog(
                {
                  ...reservation,
                  customerId: result.customerId,
                  hallId: result.hallId,
                  eventDate: result.eventDate,
                  startTime: result.startTime,
                  endTime: result.endTime,
                  guestsCount: result.guestsCount,
                  serviceIds: result.serviceIds ?? [],
                },
                getAbpErrorMessage(error)
              );
            },
          });
      });
  }

  deleteReservation(id: string): void {
    this.dialogService
      .confirm({
        type: 'delete',
        title: 'حذف الحجز',
        message: 'هل أنت متأكد من حذف هذا الحجز؟',
        warningMessage: 'تحذير: لا يمكن التراجع عن هذه العملية.',
      })
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.reservationService.deleteReservation(id).subscribe({
          next: () => {
            this.loadData();
          },
          error: (error) => {
            console.error(getAbpErrorMessage(error));
          },
        });
      });
  }
}
