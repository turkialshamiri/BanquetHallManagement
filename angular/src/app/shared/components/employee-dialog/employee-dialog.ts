import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CreateEmployee, Employee, UpdateEmployee } from 'src/app/core/models/employee.model';
import {
  getConfirmPasswordValidationMessage,
  getIdentityPasswordValidationMessage,
  isIdentityPasswordValid,
} from 'src/app/core/validators/identity-password.validator';
import { PasswordRulesHint } from '../password-rules-hint/password-rules-hint';

type EmployeeRole = 'Admin' | 'Employee';

export interface EmployeeFormState {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
  name: string;
  surname: string;
  phoneNumber: string;
  isActive: boolean;
  role: EmployeeRole;
}

export interface EmployeeDialogData {
  mode: 'create' | 'edit';
  employee?: Employee;
  initialForm?: Partial<EmployeeFormState>;
  apiError?: string;
}

interface EmployeeFormErrors {
  userName?: string;
  email?: string;
  password?: string;
  confirmPassword?: string;
  role?: string;
}

@Component({
  selector: 'app-employee-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    PasswordRulesHint,
  ],
  templateUrl: './employee-dialog.html',
  styleUrl: './employee-dialog.scss',
})
export class EmployeeDialog implements OnInit {
  dialogRef = inject(MatDialogRef<EmployeeDialog>);
  data = inject<EmployeeDialogData>(MAT_DIALOG_DATA);

  isEditMode = false;
  apiError: string | null = null;
  showPassword = false;
  showConfirmPassword = false;

  form: EmployeeFormState = {
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
    name: '',
    surname: '',
    phoneNumber: '',
    isActive: true,
    role: 'Employee',
  };

  errors: EmployeeFormErrors = {};

  ngOnInit(): void {
    this.isEditMode = this.data.mode === 'edit';
    this.apiError = this.data.apiError ?? null;

    if (this.data.initialForm) {
      this.form = {
        ...this.form,
        ...this.data.initialForm,
        confirmPassword: this.data.initialForm.confirmPassword ?? '',
      };
      return;
    }

    if (this.isEditMode && this.data.employee) {
      const e = this.data.employee;
      this.form = {
        userName: e.userName ?? '',
        email: e.email ?? '',
        password: '',
        confirmPassword: '',
        name: e.name ?? '',
        surname: e.surname ?? '',
        phoneNumber: e.phoneNumber ?? '',
        isActive: e.isActive ?? true,
        role: (e.roles?.includes('Admin') ? 'Admin' : 'Employee') as EmployeeRole,
      };
    }
  }

  save(): void {
    this.apiError = null;

    if (!this.validate()) {
      return;
    }

    if (this.isEditMode) {
      const payload: UpdateEmployee = {
        email: this.form.email.trim(),
        name: this.form.name.trim(),
        surname: this.form.surname.trim(),
        phoneNumber: this.form.phoneNumber.trim(),
        isActive: this.form.isActive,
        role: this.form.role,
      };
      this.dialogRef.close(payload);
      return;
    }

    const payload: CreateEmployee = {
      userName: this.form.userName.trim(),
      email: this.form.email.trim(),
      password: this.form.password,
      name: this.form.name.trim(),
      surname: this.form.surname.trim(),
      phoneNumber: this.form.phoneNumber.trim(),
      role: this.form.role,
    };

    this.dialogRef.close(payload);
  }

  close(): void {
    this.dialogRef.close();
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  clearError(field: keyof EmployeeFormErrors): void {
    if (this.errors[field]) {
      this.errors = { ...this.errors, [field]: undefined };
    }

    if (this.apiError) {
      this.apiError = null;
    }
  }

  private validate(): boolean {
    const errors: EmployeeFormErrors = {};

    if (!this.isEditMode && !this.form.userName.trim()) {
      errors.userName = 'اسم المستخدم مطلوب';
    }

    if (!this.form.email.trim()) {
      errors.email = 'البريد الإلكتروني مطلوب';
    }

    if (!this.isEditMode) {
      if (!this.form.password) {
        errors.password = 'كلمة المرور مطلوبة';
      } else if (!isIdentityPasswordValid(this.form.password)) {
        errors.password =
          getIdentityPasswordValidationMessage(this.form.password) ??
          'كلمة المرور لا تستوفي متطلبات الأمان';
      }

      const confirmError = getConfirmPasswordValidationMessage(
        this.form.password,
        this.form.confirmPassword
      );
      if (confirmError) {
        errors.confirmPassword = confirmError;
      }
    }

    if (!this.form.role) {
      errors.role = 'الدور مطلوب';
    }

    this.errors = errors;
    return Object.keys(errors).length === 0;
  }
}
