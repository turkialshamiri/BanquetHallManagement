import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { ConfirmDialog } from '../components/confirm-dialog/confirm-dialog';
import { ConfirmDialogData } from '../components/confirm-dialog/confirm-dialog.model';

@Injectable({
    providedIn: 'root'
})
export class DialogService {

    private dialog = inject(MatDialog);

    confirm(
        titleOrOptions: string | ConfirmDialogData,
        message?: string
    ): Observable<boolean | undefined> {
        const data: ConfirmDialogData =
            typeof titleOrOptions === 'string'
                ? {
                    title: titleOrOptions,
                    message: message ?? '',
                    type: 'delete',
                }
                : {
                    type: 'delete',
                    ...titleOrOptions,
                };

        return this.dialog.open(ConfirmDialog, {
            width: '420px',
            disableClose: true,
            data,
        }).afterClosed();
    }
}
