import { Component } from '@angular/core';

@Component({
  selector: 'app-statistics-cards',
  standalone: true,
  templateUrl: './statistics-cards.html',
  styleUrl: './statistics-cards.scss'
})
export class StatisticsCardsComponent {

  totalHalls = 25;

  totalBookings = 180;

  completedBookings = 142;

  pendingBookings = 38;

}