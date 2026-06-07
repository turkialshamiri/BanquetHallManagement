import {
  AfterViewInit,
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { ReservationsTableComponent } from './components/reservations-table/reservations-table';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [Sidebar, Navbar, ReservationsTableComponent],
  templateUrl: './bookings.html',
  styleUrl: './bookings.scss',
})
export class Bookings implements AfterViewInit {
  @ViewChild(ReservationsTableComponent)
  reservationsTable!: ReservationsTableComponent;

  private route = inject(ActivatedRoute);
  private router = inject(Router);

  ngAfterViewInit(): void {
    const shouldOpenAddDialog =
      this.route.snapshot.data['openAddDialog'] === true;

    if (!shouldOpenAddDialog) {
      return;
    }

    this.reservationsTable.openAddReservationDialog();

    this.router.navigate(['/bookings'], { replaceUrl: true });
  }
}
