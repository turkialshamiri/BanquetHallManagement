import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Hall } from '../../../../core/models/hall.model';
import { MatIconModule } from '@angular/material/icon';
import { OnInit } from '@angular/core';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';

import { HallService } from 'src/app/core/services/hall.service';

@Component({
  selector: 'app-halls-table',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './halls-table.html',
  styleUrl: './halls-table.scss'
})
export class HallsTableComponent implements OnInit {

  private hallService =
    inject(HallService);

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

  console.log('Edit Hall:', id);

}

deleteHall(id: string): void {

  console.log('Delete Hall:', id);

}

}