import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog } from '../components/confirm-dialog/confirm-dialog';

@Injectable({
    providedIn: 'root'
})
export class DialogService {

    private dialog = inject(MatDialog);

    confirm(
        title: string,
        message: string
    ) {

        return this.dialog.open(ConfirmDialog, {
            width: '420px',
            disableClose: true,
            data: {
                title,
                message
            }
        }).afterClosed();
    }
}