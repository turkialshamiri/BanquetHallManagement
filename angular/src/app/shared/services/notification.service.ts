import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private snackBar = inject(MatSnackBar);

  showError(message: string, duration = 7000): void {
    this.snackBar.open(message, 'إغلاق', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['app-snackbar-error'],
    });
  }

  showSuccess(message: string, duration = 4000): void {
    this.snackBar.open(message, 'إغلاق', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['app-snackbar-success'],
    });
  }
}
