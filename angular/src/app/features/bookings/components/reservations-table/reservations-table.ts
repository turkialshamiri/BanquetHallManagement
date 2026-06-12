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
import { forkJoin } from 'rxjs';
import { Customer } from 'src/app/core/models/customer.model';
import { Hall } from 'src/app/core/models/hall.model';
import {
  CreateUpdateReservation,
  Reservation,
  toTimeInputValue,
} from 'src/app/core/models/reservation.model';
import { ReservationService } from 'src/app/core/services/reservation.service';
import { PaymentService } from 'src/app/core/services/payment.service';
import { CustomerService } from 'src/app/core/services/customer.service';
import { HallService } from 'src/app/core/services/hall.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import {
  canCancelReservation,
  canDeleteReservation,
  canEditReservation,
  getReservationStatusClass,
} from 'src/app/core/utils/reservation-status.util';
import {
  calculatePaymentPercentage,
  canRecordPayment,
  getRemainingAmount,
} from 'src/app/core/utils/payment.util';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';
import {
  ADD_RESERVATION_DIALOG_CONFIG,
  AddReservationDialog,
  AddReservationDialogData,
} from 'src/app/shared/components/add-reservation-dialog/add-reservation-dialog';
import {
  RECORD_PAYMENT_DIALOG_CONFIG,
  RecordPaymentDialog,
  RecordPaymentDialogData,
  RecordPaymentDialogResult,
} from 'src/app/shared/components/record-payment-dialog/record-payment-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { PolicyService } from 'src/app/core/services/policy.service';

@Component({
  selector: 'app-reservations-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule, AppLocalizationPipe],
  templateUrl: './reservations-table.html',
  styleUrl: './reservations-table.scss',
})
export class ReservationsTableComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private paymentService = inject(PaymentService);
  private router = inject(Router);
  private customerService = inject(CustomerService);
  private hallService = inject(HallService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  readonly statusL10n = inject(StatusLocalizationService);

  readonly reservations = signal<Reservation[]>([]);
  customersMap = new Map<string, Customer>();
  hallsMap = new Map<string, Hall>();

  getReservationStatusClass = getReservationStatusClass;
  canCancelReservation = canCancelReservation;
  canEditReservation = canEditReservation;
  canDeleteReservation = canDeleteReservation;
  canRecordPayment = canRecordPayment;
  getRemainingAmount = getRemainingAmount;
  calculatePaymentPercentage = calculatePaymentPercentage;
  formatTime = toTimeInputValue;

  canCreate = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Create');
  canUpdate = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Update');
  canDelete = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Delete');
  canCancel = this.policy.hasSnapshot('BanquetHallManagement.Reservations.Cancel');
  canRecordPaymentAction = this.policy.hasSnapshot(
    'BanquetHallManagement.Reservations.RecordPayment'
  );

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
        this.reservations.set(reservations.items);
        this.customersMap = new Map(
          customers.items.map((customer) => [customer.id, customer])
        );
        this.hallsMap = new Map(
          halls.items.map((hall) => [hall.id, hall])
        );
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Reservations:LoadFailed'))
        );
      },
    });
  }

  reservationStatusLabel(status: string): string {
    return this.statusL10n.reservationStatus(status);
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
      ...ADD_RESERVATION_DIALOG_CONFIG,
      data: initialData,
    });

    dialogRef
      .afterClosed()
      .subscribe((result: CreateUpdateReservation | undefined) => {
        if (!result) {
          leaveCreateRoute(this.router, '/bookings/create', '/bookings');
          return;
        }

        this.reservationService.createReservation(result).subscribe({
          next: () => {
            this.loadData();
            leaveCreateRoute(this.router, '/bookings/create', '/bookings');
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

  openRecordPaymentDialog(reservation: Reservation, apiError?: string): void {
    const dialogRef = this.dialog.open(RecordPaymentDialog, {
      ...RECORD_PAYMENT_DIALOG_CONFIG,
      data: {
        reservation,
        customerName: this.getCustomerName(reservation.customerId),
        hallName: this.getHallName(reservation.hallId),
        apiError,
      } satisfies RecordPaymentDialogData,
    });

    dialogRef
      .afterClosed()
      .subscribe((result: RecordPaymentDialogResult | undefined) => {
        if (!result) {
          return;
        }

        const request$ = result.isDeposit
          ? this.paymentService.recordDeposit({
              reservationId: reservation.id,
              amount: result.amount,
            })
          : this.paymentService.recordInstallment({
              reservationId: reservation.id,
              amount: result.amount,
            });

        request$.subscribe({
          next: (paymentResult) => {
            this.notification.showSuccess(
              this.l10n.instant('Finance:Payment:Success')
            );
            this.loadData();

            const hallAccessCardId = paymentResult.hallAccessCardId ?? null;

            if (hallAccessCardId) {
              void this.router.navigate([
                '/finance/access-cards',
                hallAccessCardId,
              ]);
            }
          },
          error: (error) => {
            this.openRecordPaymentDialog(reservation, getAbpErrorMessage(error));
          },
        });
      });
  }

  editReservation(id: string): void {
    const reservation = this.reservations().find((item) => item.id === id);

    if (!reservation || !canEditReservation(reservation.status)) {
      return;
    }

    this.openEditReservationDialog(reservation);
  }

  cancelReservation(id: string): void {
    this.dialogService
      .confirm({
        type: 'cancel',
        title: this.l10n.instant('Reservations:Cancel:Title'),
        message: this.l10n.instant('Reservations:Cancel:Message'),
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
            this.notification.showError(getAbpErrorMessage(error));
          },
        });
      });
  }

  private openEditReservationDialog(
    reservation: Reservation,
    apiError?: string
  ): void {
    const dialogRef = this.dialog.open(AddReservationDialog, {
      ...ADD_RESERVATION_DIALOG_CONFIG,
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
        title: this.l10n.instant('Reservations:Delete:Title'),
        message: this.l10n.instant('Reservations:Delete:Message'),
        warningMessage: this.l10n.instant('Reservations:Delete:Warning'),
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
            this.notification.showError(getAbpErrorMessage(error));
          },
        });
      });
  }
}
