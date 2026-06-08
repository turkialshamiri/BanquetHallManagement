import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
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
  readonly data = resolveConfirmDialogData(
    inject<ConfirmDialogData>(MAT_DIALOG_DATA)
  );

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
