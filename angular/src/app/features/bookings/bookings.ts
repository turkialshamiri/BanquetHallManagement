import {
  AfterViewInit,
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ReservationsTableComponent } from './components/reservations-table/reservations-table';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [ReservationsTableComponent],
  templateUrl: './bookings.html',
  styleUrl: './bookings.scss',
})
export class Bookings implements AfterViewInit {
  @ViewChild(ReservationsTableComponent)
  reservationsTable!: ReservationsTableComponent;

  private route = inject(ActivatedRoute);

  ngAfterViewInit(): void {
    const shouldOpenAddDialog =
      this.route.snapshot.data['openAddDialog'] === true;

    if (!shouldOpenAddDialog) {
      return;
    }

    this.reservationsTable.openAddReservationDialog();
  }
}
