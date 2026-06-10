import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import {
  ConfirmDialogData,
  resolveConfirmDialogData,
} from './confirm-dialog.model';

@Component({
  selector: 'app-confirm-dialog',
  imports: [MatIconModule],
  standalone: true,
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.scss'
})
export class ConfirmDialog {

  readonly dialogRef = inject(MatDialogRef<ConfirmDialog>);
  private l10n = inject(AppLocalizationService);
  readonly data = resolveConfirmDialogData(
    inject<ConfirmDialogData>(MAT_DIALOG_DATA),
    this.l10n
  );

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
