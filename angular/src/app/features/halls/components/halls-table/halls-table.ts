import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Hall } from 'src/app/core/models/hall.model';
import { HallService } from 'src/app/core/services/hall.service';
import { AddHallDialog } from 'src/app/shared/components/add-hall-dialog/add-hall-dialog';
import { DialogService } from 'src/app/shared/services/dialog.service';

@Component({
  selector: 'app-halls-table',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDialogModule],
  templateUrl: './halls-table.html',
  styleUrl: './halls-table.scss',
})
export class HallsTableComponent implements OnInit {
  private hallService = inject(HallService);
  private dialog = inject(MatDialog);
  private dialogService = inject(DialogService);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  halls: Hall[] = [];
  pageTitle = 'القاعات';
  pageSubtitle = 'جميع القاعات المسجلة بالنظام';
  statusFilter: number | null = null;

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.applyRouteConfig(data);
      this.loadHalls();
      this.cdr.markForCheck();
    });
  }

  loadHalls(): void {
    this.hallService.getHalls().subscribe({
      next: (response) => {
        let items: Hall[] = response.items;

        if (this.statusFilter != null) {
          items = items.filter(
            (hall) => hall.status === this.statusFilter
          );
        }

        this.halls = items;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error(error);
      },
    });
  }

  openAddHallDialog(): void {
    const dialogRef = this.dialog.open(AddHallDialog, {
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.hallService.createHall(result).subscribe({
        next: () => {
          this.loadHalls();
        },
        error: (error) => {
          console.error(error);
        },
      });
    });
  }

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
    const hall = this.halls.find((h) => h.id === id);

    if (!hall) {
      return;
    }

    const dialogRef = this.dialog.open(AddHallDialog, {
      width: '700px',
      disableClose: true,
      data: hall,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.hallService.updateHall(id, result).subscribe({
        next: () => {
          this.loadHalls();
        },
        error: (error) => {
          console.error(error);
        },
      });
    });
  }

  deleteHall(id: string): void {
    this.dialogService
      .confirm(
        'حذف القاعة',
        'هل أنت متأكد من حذف هذه القاعة؟ لا يمكن التراجع عن العملية.'
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.hallService.deleteHall(id).subscribe({
          next: () => {
            this.loadHalls();
          },
          error: (error) => {
            console.error(error);
          },
        });
      });
  }

  private applyRouteConfig(
    data: Record<string, unknown> = this.route.snapshot.data
  ): void {
    this.pageTitle = (data['pageTitle'] as string) ?? 'القاعات';
    this.pageSubtitle =
      (data['pageSubtitle'] as string) ?? 'جميع القاعات المسجلة بالنظام';
    this.statusFilter = (data['statusFilter'] as number | null) ?? null;
  }
}
