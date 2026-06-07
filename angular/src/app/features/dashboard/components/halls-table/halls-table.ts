import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Hall } from '../../../../core/models/hall.model';
import { MatIconModule } from '@angular/material/icon';
import { OnInit } from '@angular/core';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AddHallDialog } from 'src/app/shared/components/add-hall-dialog/add-hall-dialog';

import { HallService } from 'src/app/core/services/hall.service';
import { DialogService } from 'src/app/shared/services/dialog.service';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';


@Component({
  selector: 'app-halls-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule],
  templateUrl: './halls-table.html',
  styleUrl: './halls-table.scss'
})
export class HallsTableComponent implements OnInit {

  private hallService =
    inject(HallService);
  private dialog = inject(MatDialog);


  halls: Hall[] = [];

  ngOnInit(): void {

    this.loadHalls();

  }

  loadHalls(): void {

    this.hallService
      .getHalls()
      .subscribe({

        next: (response) => {

          this.halls =
            response.items;

          console.log(this.halls);

        },

        error: (error) => {

          console.error(error);

        }

      });

  }

  openAddHallDialog(): void {

  const dialogRef = this.dialog.open(
    AddHallDialog,
    {
      disableClose: true
    }
  );

  dialogRef.afterClosed()
    .subscribe(result => {

      if (!result) return;

      this.hallService
        .createHall(result)
        .subscribe(() => {

          this.loadHalls();

        });

    });

}

  private dialogService = inject(DialogService);

  getStatusText(status: number): string {

    switch (status) {

      case 1:
        return 'متاحة';

      case 2:
        return 'محجوزة';

      case 3:
        return 'مشغولة';

      case 4:
        return 'تحت الصيانة';

      default:
        return 'غير معروف';
    }
  }

  getTypeText(type: number): string {

    switch (type) {

      case 1:
        return 'أفراح';

      case 2:
        return 'مؤتمرات';

      case 3:
        return 'اجتماعات';

      default:
        return 'غير معروف';
    }
  }

editHall(id: string): void {

  const hall = this.halls.find(
    h => h.id === id
  );

  if (!hall) return;

  const dialogRef = this.dialog.open(
    AddHallDialog,
    {
      width: '700px',
      disableClose: true,
      data: hall
    }
  );

  dialogRef.afterClosed()
    .subscribe(result => {

      if (!result) return;

      this.hallService
        .updateHall(id, result)
        .subscribe(() => {

          this.loadHalls();

        });

    });

}

  deleteHall(id: string): void {

    this.dialogService
      .confirm(
        'حذف القاعة',
        'هل أنت متأكد من حذف هذه القاعة؟ لا يمكن التراجع عن العملية.'
      )
      .subscribe(result => {

        if (!result) return;

        this.hallService.deleteHall(id)
          .subscribe(() => {

            this.loadHalls();

          });

      });

  }

}