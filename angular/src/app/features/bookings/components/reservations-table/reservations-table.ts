import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  MatDatepickerModule,
} from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
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
  calculateDurationLabel,
  resolvePaymentStatusCode,
  toTimeInputValue,
} from 'src/app/core/models/reservation.model';
import { ReservationService } from 'src/app/core/services/reservation.service';
import { PaymentService } from 'src/app/core/services/payment.service';
import { CustomerService } from 'src/app/core/services/customer.service';
import { HallService } from 'src/app/core/services/hall.service';
import { ServiceService } from 'src/app/core/services/service.service';
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
  canViewSettlementInvoice,
  getRemainingAmount,
} from 'src/app/core/utils/payment.util';
import { InvoiceService } from 'src/app/core/services/invoice.service';
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
import { ReservationDetailsDialog } from 'src/app/shared/components/reservation-details-dialog/reservation-details-dialog';
import {
  RESERVATION_DETAILS_DIALOG_CONFIG,
  ReservationDetailsViewModel,
  ReservationServiceLine,
} from 'src/app/shared/components/reservation-details-dialog/reservation-details-dialog.model';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { PolicyService } from 'src/app/core/services/policy.service';
import { DEFAULT_PAGE_SIZE } from 'src/app/core/constants/pagination.constants';
import { getSkipCount } from 'src/app/core/utils/pagination.util';
import { DataTablePaginationComponent } from 'src/app/shared/components/data-table-pagination/data-table-pagination';
import {
  EMPTY_RESERVATION_FILTER,
  ReservationFilter,
} from 'src/app/core/models/reservation-filter.model';
import { RESERVATION_STATUS } from 'src/app/core/utils/reservation-status.util';
import {
  DatePresetId,
  formatDateForFilter,
  parseFilterDate,
  resolveDatePreset,
} from 'src/app/core/utils/date-preset.util';
import {
  ReservationFilterChip,
  ReservationFilterChipType,
} from 'src/app/core/models/reservation-filter-chip.model';

@Component({
  selector: 'app-reservations-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatDialogModule,
    AppLocalizationPipe,
    DataTablePaginationComponent,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './reservations-table.html',
  styleUrl: './reservations-table.scss',
})
export class ReservationsTableComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private paymentService = inject(PaymentService);
  private invoiceService = inject(InvoiceService);
  private router = inject(Router);
  private customerService = inject(CustomerService);
  private hallService = inject(HallService);
  private serviceService = inject(ServiceService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  readonly statusL10n = inject(StatusLocalizationService);

  readonly reservations = signal<Reservation[]>([]);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly filterDraft = signal<ReservationFilter>({ ...EMPTY_RESERVATION_FILTER });
  readonly appliedFilters = signal<ReservationFilter>({ ...EMPTY_RESERVATION_FILTER });
  readonly customerSearch = signal('');
  readonly dateRangeStart = signal<Date | null>(null);
  readonly dateRangeEnd = signal<Date | null>(null);
  readonly activeDatePreset = signal<DatePresetId | null>(null);
  readonly halls = signal<Hall[]>([]);
  readonly customers = signal<Customer[]>([]);
  readonly detailsLoadingId = signal<string | null>(null);
  customersMap = new Map<string, Customer>();
  hallsMap = new Map<string, Hall>();

  readonly statusFilterOptions = [
    RESERVATION_STATUS.Pending,
    RESERVATION_STATUS.Confirmed,
    RESERVATION_STATUS.FullyPaid,
    RESERVATION_STATUS.Completed,
    RESERVATION_STATUS.Cancelled,
  ] as const;

  readonly datePresets: DatePresetId[] = [
    'today',
    'thisWeek',
    'thisMonth',
    'last30Days',
    'thisYear',
    'all',
  ];

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
  canViewInvoiceAction = this.policy.hasSnapshot(
    'BanquetHallManagement.Finance.Invoices.View'
  );

  canViewSettlementInvoice = canViewSettlementInvoice;

  ngOnInit(): void {
    this.loadLookups();
    this.loadData();
  }

  loadLookups(): void {
    forkJoin({
      customers: this.customerService.getCustomers(),
      halls: this.hallService.getHalls(),
    }).subscribe({
      next: ({ customers, halls }) => {
        this.customers.set(customers.items);
        this.halls.set(halls.items);
        this.customersMap = new Map(
          customers.items.map((customer) => [customer.id, customer])
        );
        this.hallsMap = new Map(halls.items.map((hall) => [hall.id, hall]));
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Reservations:LoadFailed'))
        );
      },
    });
  }

  loadData(): void {
    const skip = getSkipCount(this.pageIndex(), this.pageSize());
    this.loading.set(true);

    this.reservationService
      .getReservations(skip, this.pageSize(), this.appliedFilters())
      .subscribe({
        next: (reservations) => {
          this.reservations.set(reservations.items);
          this.totalCount.set(reservations.totalCount);
          this.loading.set(false);
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.loading.set(false);
          this.notification.showError(
            getAbpErrorMessage(error, this.l10n.instant('Reservations:LoadFailed'))
          );
        },
      });
  }

  applyFilters(): void {
    this.appliedFilters.set({ ...this.filterDraft() });
    this.syncDateRangeFromFilter(this.filterDraft());
    this.pageIndex.set(0);
    this.loadData();
  }

  resetFilters(): void {
    this.filterDraft.set({ ...EMPTY_RESERVATION_FILTER });
    this.appliedFilters.set({ ...EMPTY_RESERVATION_FILTER });
    this.customerSearch.set('');
    this.dateRangeStart.set(null);
    this.dateRangeEnd.set(null);
    this.activeDatePreset.set(null);
    this.pageIndex.set(0);
    this.loadData();
  }

  updateFilterDraft<K extends keyof ReservationFilter>(
    key: K,
    value: ReservationFilter[K]
  ): void {
    this.filterDraft.update((current) => ({ ...current, [key]: value }));
  }

  filteredCustomers(): Customer[] {
    const term = this.customerSearch().trim().toLowerCase();
    const list = this.customers();

    if (!term) {
      return list;
    }

    return list.filter(
      (customer) =>
        customer.name.toLowerCase().includes(term) ||
        customer.phone.toLowerCase().includes(term)
    );
  }

  onCustomerSelected(customerId: string | null): void {
    this.updateFilterDraft('customerId', customerId);

    if (!customerId) {
      this.customerSearch.set('');
      return;
    }

    const customer = this.customersMap.get(customerId);
    this.customerSearch.set(customer?.name ?? '');
  }

  onDateRangeStartChange(value: Date | null): void {
    this.activeDatePreset.set(null);
    this.dateRangeStart.set(value);
    this.updateFilterDraft('eventDateFrom', formatDateForFilter(value));
  }

  onDateRangeEndChange(value: Date | null): void {
    this.activeDatePreset.set(null);
    this.dateRangeEnd.set(value);
    this.updateFilterDraft('eventDateTo', formatDateForFilter(value));
  }

  applyDatePreset(preset: DatePresetId): void {
    const range = resolveDatePreset(preset);
    this.activeDatePreset.set(preset);
    this.dateRangeStart.set(range.from);
    this.dateRangeEnd.set(range.to);
    this.updateFilterDraft('eventDateFrom', formatDateForFilter(range.from));
    this.updateFilterDraft('eventDateTo', formatDateForFilter(range.to));
  }

  datePresetLabel(preset: DatePresetId): string {
    const keyMap: Record<DatePresetId, string> = {
      today: 'Reservations:Filters:Preset:Today',
      thisWeek: 'Reservations:Filters:Preset:ThisWeek',
      thisMonth: 'Reservations:Filters:Preset:ThisMonth',
      last30Days: 'Reservations:Filters:Preset:Last30Days',
      thisYear: 'Reservations:Filters:Preset:ThisYear',
      all: 'Reservations:Filters:Preset:AllDates',
    };

    return this.l10n.instant(keyMap[preset]);
  }

  dateRangeDisplayLabel(): string {
    const from = this.filterDraft().eventDateFrom;
    const to = this.filterDraft().eventDateTo;

    if (!from && !to) {
      return this.l10n.instant('Reservations:Filters:DateRange');
    }

    const format = (value: string) => value.replaceAll('-', '/');

    if (from && to) {
      return `${format(from)} - ${format(to)}`;
    }

    if (from) {
      return `${format(from)} -`;
    }

    return `- ${format(to!)}`;
  }

  chipDisplayLabel(chip: ReservationFilterChip): string {
    if (chip.type === 'date') {
      return chip.label;
    }

    const prefixMap: Record<Exclude<ReservationFilterChipType, 'date'>, string> = {
      hall: this.l10n.instant('Reservations:Filters:Hall'),
      customer: this.l10n.instant('Reservations:Filters:Customer'),
      status: this.l10n.instant('Reservations:Filters:Status'),
      reservationNumber: this.l10n.instant('Reservations:Filters:ReservationNumber'),
    };

    return `${prefixMap[chip.type]}: ${chip.label}`;
  }

  chipIcon(type: ReservationFilterChipType): string {
    const icons: Record<ReservationFilterChipType, string> = {
      hall: 'apartment',
      customer: 'person',
      date: 'calendar_today',
      status: 'flag',
      reservationNumber: 'tag',
    };

    return icons[type];
  }

  hasActiveFilters(): boolean {
    const filters = this.appliedFilters();

    return !!(
      filters.hallId ||
      filters.customerId ||
      filters.eventDateFrom ||
      filters.eventDateTo ||
      filters.reservationNumber?.trim() ||
      filters.status
    );
  }

  activeFilterChips(): ReservationFilterChip[] {
    const filters = this.appliedFilters();
    const chips: ReservationFilterChip[] = [];

    if (filters.hallId) {
      chips.push({
        type: 'hall',
        label: this.getHallName(filters.hallId),
      });
    }

    if (filters.customerId) {
      chips.push({
        type: 'customer',
        label: this.getCustomerName(filters.customerId),
      });
    }

    if (filters.eventDateFrom || filters.eventDateTo) {
      chips.push({
        type: 'date',
        label: this.dateRangeSummary(filters),
      });
    }

    if (filters.status) {
      chips.push({
        type: 'status',
        label: this.reservationStatusLabel(filters.status),
      });
    }

    if (filters.reservationNumber?.trim()) {
      chips.push({
        type: 'reservationNumber',
        label: filters.reservationNumber.trim(),
      });
    }

    return chips;
  }

  removeFilterChip(type: ReservationFilterChipType): void {
    const patch: Partial<ReservationFilter> = {};

    switch (type) {
      case 'hall':
        patch.hallId = null;
        break;
      case 'customer':
        patch.customerId = null;
        this.customerSearch.set('');
        break;
      case 'date':
        patch.eventDateFrom = null;
        patch.eventDateTo = null;
        this.dateRangeStart.set(null);
        this.dateRangeEnd.set(null);
        this.activeDatePreset.set(null);
        break;
      case 'status':
        patch.status = null;
        break;
      case 'reservationNumber':
        patch.reservationNumber = null;
        break;
    }

    this.filterDraft.update((current) => ({ ...current, ...patch }));
    this.appliedFilters.update((current) => ({ ...current, ...patch }));
    this.pageIndex.set(0);
    this.loadData();
  }

  private syncDateRangeFromFilter(filter: ReservationFilter): void {
    this.dateRangeStart.set(parseFilterDate(filter.eventDateFrom));
    this.dateRangeEnd.set(parseFilterDate(filter.eventDateTo));
  }

  private dateRangeSummary(filters: ReservationFilter): string {
    const { eventDateFrom, eventDateTo } = filters;

    if (!eventDateFrom && !eventDateTo) {
      return this.l10n.instant('Reservations:Filters:AllDates');
    }

    const format = (value: string) => value.replaceAll('-', '/');

    if (eventDateFrom && eventDateTo) {
      return `${format(eventDateFrom)} - ${format(eventDateTo)}`;
    }

    if (eventDateFrom) {
      return `${format(eventDateFrom)} -`;
    }

    return `- ${format(eventDateTo!)}`;
  }

  recordsSummaryLabel(): string {
    return this.l10n.instant(
      'Reservations:Filters:Summary:Records',
      String(this.totalCount())
    );
  }

  onPageChange(pageIndex: number): void {
    this.pageIndex.set(pageIndex);
    this.loadData();
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

  viewDetails(reservation: Reservation): void {
    this.detailsLoadingId.set(reservation.id);

    forkJoin({
      reservation: this.reservationService.getReservation(reservation.id),
      payments: this.paymentService.getByReservation(reservation.id),
      services: this.serviceService.getServices(),
    }).subscribe({
      next: ({ reservation: detailsReservation, payments, services }) => {
        this.detailsLoadingId.set(null);

        const customer = this.customersMap.get(detailsReservation.customerId) ?? null;
        const hall = this.hallsMap.get(detailsReservation.hallId) ?? null;
        const serviceMap = new Map(
          services.items.map((service) => [service.id, service])
        );

        const depositAmount = payments.items
          .filter((payment) => payment.paymentType === 'Deposit')
          .reduce((sum, payment) => sum + payment.amount, 0);

        const serviceLines: ReservationServiceLine[] = (detailsReservation.serviceIds ?? [])
          .map((serviceId) => {
            const service = serviceMap.get(serviceId);
            if (!service) {
              return null;
            }

            return {
              serviceId,
              name: service.name,
              quantity: 1,
              price: service.price,
              total: service.price,
            };
          })
          .filter((line): line is ReservationServiceLine => line != null);

        const details: ReservationDetailsViewModel = {
          reservation: detailsReservation,
          customer,
          hall,
          payments: payments.items,
          depositAmount,
          paymentStatusCode: resolvePaymentStatusCode(
            detailsReservation.paidAmount ?? 0,
            detailsReservation.totalPrice
          ),
          services: serviceLines,
          durationLabel: calculateDurationLabel(
            detailsReservation.startTime,
            detailsReservation.endTime
          ),
        };

        this.dialog
          .open(ReservationDetailsDialog, {
            ...RESERVATION_DETAILS_DIALOG_CONFIG,
            data: {
              details,
              canArchive: this.canDelete,
            },
          })
          .afterClosed()
          .subscribe((result) => {
            if (result === 'archived') {
              this.loadData();
            }
          });
      },
      error: (error) => {
        this.detailsLoadingId.set(null);
        this.notification.showError(getAbpErrorMessage(error));
      },
    });
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

  viewSettlementInvoice(reservation: Reservation): void {
    this.invoiceService.getSettlementByReservation(reservation.id).subscribe({
      next: (invoice) => {
        void this.router.navigate(['/finance/invoices', invoice.id]);
      },
      error: (error) => {
        this.notification.showError(
          getAbpErrorMessage(
            error,
            this.l10n.instant('Finance:Invoices:Detail:NotFound')
          )
        );
      },
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
