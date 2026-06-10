import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { Employee, CreateEmployee, UpdateEmployee } from 'src/app/core/models/employee.model';
import { EmployeeService } from 'src/app/core/services/employee.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { NotificationService } from 'src/app/shared/services/notification.service';
import {
  EmployeeDialog,
  EmployeeFormState,
} from 'src/app/shared/components/employee-dialog/employee-dialog';
import { ResetPasswordDialog } from 'src/app/shared/components/reset-password-dialog/reset-password-dialog';
import { PolicyService } from 'src/app/core/services/policy.service';
import { getFriendlyIdentityErrorMessage } from 'src/app/core/utils/identity-error.util';
import { formatRoleLabels } from 'src/app/core/utils/role-label.util';
import { StatusLocalizationService } from 'src/app/core/services/status-localization.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule, AppLocalizationPipe],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit {
  private employeeService = inject(EmployeeService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);
  private policy = inject(PolicyService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  private statusL10n = inject(StatusLocalizationService);

  employees: Employee[] = [];
  isLoading = false;
  loadError: string | null = null;

  canCreate = this.policy.hasSnapshot('BanquetHallManagement.Users.Create');
  canUpdate = this.policy.hasSnapshot('BanquetHallManagement.Users.Update');
  canDelete = this.policy.hasSnapshot('BanquetHallManagement.Users.Delete');
  canResetPassword = this.policy.hasSnapshot('BanquetHallManagement.Users.ResetPassword');
  canActivate = this.policy.hasSnapshot('BanquetHallManagement.Users.Activate');
  canDeactivate = this.policy.hasSnapshot('BanquetHallManagement.Users.Deactivate');

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading = true;
    this.loadError = null;
    this.employeeService.getEmployees().subscribe({
      next: (res) => {
        this.employees = res.items;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.loadError = getAbpErrorMessage(
          err,
          this.l10n.instant('Users:LoadFailed')
        );
        this.cdr.markForCheck();
      },
    });
  }

  openCreateDialog(): void {
    this.openCreateDialogWithState();
  }

  private openCreateDialogWithState(
    initialForm?: Partial<EmployeeFormState>,
    apiError?: string
  ): void {
    const ref = this.dialog.open(EmployeeDialog, {
      width: '720px',
      disableClose: true,
      data: { mode: 'create', initialForm, apiError },
    });

    ref.afterClosed().subscribe((payload: CreateEmployee | undefined) => {
      if (!payload) return;

      this.employeeService.createEmployee(payload).subscribe({
        next: () => this.loadEmployees(),
        error: (err) => {
          this.openCreateDialogWithState(
            {
              userName: payload.userName,
              email: payload.email,
              password: payload.password,
              confirmPassword: payload.password,
              name: payload.name,
              surname: payload.surname,
              phoneNumber: payload.phoneNumber,
              role: payload.role as EmployeeFormState['role'],
              isActive: true,
            },
            getFriendlyIdentityErrorMessage(
              err,
              this.l10n.instant('Users:CreateFailed')
            )
          );
        },
      });
    });
  }

  openEditDialog(employee: Employee): void {
    const ref = this.dialog.open(EmployeeDialog, {
      width: '720px',
      disableClose: true,
      data: { mode: 'edit', employee },
    });

    ref.afterClosed().subscribe((payload: UpdateEmployee | undefined) => {
      if (!payload) return;

      this.employeeService.updateEmployee(employee.id, payload).subscribe({
        next: () => this.loadEmployees(),
        error: (err) => {
          this.dialog.open(EmployeeDialog, {
            width: '720px',
            disableClose: true,
            data: { mode: 'edit', employee, apiError: getAbpErrorMessage(err) },
          });
        },
      });
    });
  }

  deleteEmployee(employee: Employee): void {
    this.dialogService
      .confirm({
        type: 'delete',
        title: this.l10n.instant('Users:Delete:Title'),
        message: this.l10n.instant('Users:Delete:Message', employee.userName),
        warningMessage: this.l10n.instant('Users:Delete:Warning'),
      })
      .subscribe((confirmed) => {
        if (!confirmed) return;

        this.employeeService.deleteEmployee(employee.id).subscribe({
          next: () => this.loadEmployees(),
          error: (err) => {
            this.notification.showError(
              getAbpErrorMessage(err, this.l10n.instant('Users:DeleteFailed'))
            );
          },
        });
      });
  }

  resetPassword(employee: Employee): void {
    this.openResetPasswordDialog(employee);
  }

  private openResetPasswordDialog(
    employee: Employee,
    initialPassword?: string,
    initialConfirmPassword?: string,
    apiError?: string
  ): void {
    const ref = this.dialog.open(ResetPasswordDialog, {
      width: '520px',
      disableClose: true,
      data: {
        userName: employee.userName,
        initialPassword,
        initialConfirmPassword,
        apiError,
      },
    });

    ref.afterClosed().subscribe((result: string | undefined) => {
      if (!result) return;

      this.employeeService.resetPassword(employee.id, result).subscribe({
        next: () => {},
        error: (err) => {
          this.openResetPasswordDialog(
            employee,
            result,
            result,
            getFriendlyIdentityErrorMessage(
              err,
              this.l10n.instant('Users:ResetPasswordFailed')
            )
          );
        },
      });
    });
  }

  toggleActive(employee: Employee): void {
    const action$ = employee.isActive
      ? this.employeeService.deactivate(employee.id)
      : this.employeeService.activate(employee.id);

    action$.subscribe({
      next: () => this.loadEmployees(),
      error: (err) => {
        this.notification.showError(
          getAbpErrorMessage(err, this.l10n.instant('Users:UpdateStatusFailed'))
        );
      },
    });
  }

  roleLabel(employee: Employee): string {
    return formatRoleLabels(employee.roles, this.statusL10n);
  }
}
