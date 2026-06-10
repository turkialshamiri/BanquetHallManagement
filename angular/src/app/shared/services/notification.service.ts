import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private snackBar = inject(MatSnackBar);
  private l10n = inject(AppLocalizationService);

  showError(message: string, duration = 7000): void {
    this.snackBar.open(message, this.l10n.instant('Close'), {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['app-snackbar-error'],
    });
  }

  showSuccess(message: string, duration = 4000): void {
    this.snackBar.open(message, this.l10n.instant('Close'), {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['app-snackbar-success'],
    });
  }
}
