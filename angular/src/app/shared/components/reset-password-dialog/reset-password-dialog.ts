import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {
  getConfirmPasswordValidationMessage,
  getIdentityPasswordValidationMessage,
  isIdentityPasswordValid,
} from 'src/app/core/validators/identity-password.validator';
import { PasswordRulesHint } from '../password-rules-hint/password-rules-hint';

export interface ResetPasswordDialogData {
  userName: string;
  initialPassword?: string;
  initialConfirmPassword?: string;
  apiError?: string;
}

interface ResetPasswordErrors {
  password?: string;
  confirmPassword?: string;
}

@Component({
  selector: 'app-reset-password-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    PasswordRulesHint,
  ],
  templateUrl: './reset-password-dialog.html',
  styleUrl: './reset-password-dialog.scss',
})
export class ResetPasswordDialog implements OnInit {
  dialogRef = inject(MatDialogRef<ResetPasswordDialog>);
  data = inject<ResetPasswordDialogData>(MAT_DIALOG_DATA);

  newPassword = '';
  confirmPassword = '';
  apiError: string | null = null;
  showPassword = false;
  showConfirmPassword = false;
  errors: ResetPasswordErrors = {};

  ngOnInit(): void {
    this.newPassword = this.data.initialPassword ?? '';
    this.confirmPassword = this.data.initialConfirmPassword ?? '';
    this.apiError = this.data.apiError ?? null;
  }

  save(): void {
    this.apiError = null;

    if (!this.validate()) {
      return;
    }

    this.dialogRef.close(this.newPassword);
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

  clearError(field: keyof ResetPasswordErrors): void {
    if (this.errors[field]) {
      this.errors = { ...this.errors, [field]: undefined };
    }

    if (this.apiError) {
      this.apiError = null;
    }
  }

  private validate(): boolean {
    const errors: ResetPasswordErrors = {};

    if (!this.newPassword) {
      errors.password = 'كلمة المرور مطلوبة';
    } else if (!isIdentityPasswordValid(this.newPassword)) {
      errors.password =
        getIdentityPasswordValidationMessage(this.newPassword) ??
        'كلمة المرور لا تستوفي متطلبات الأمان';
    }

    const confirmError = getConfirmPasswordValidationMessage(
      this.newPassword,
      this.confirmPassword
    );
    if (confirmError) {
      errors.confirmPassword = confirmError;
    }

    this.errors = errors;
    return Object.keys(errors).length === 0;
  }
}
