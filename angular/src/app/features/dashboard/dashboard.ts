import { Component } from '@angular/core';
import { StatisticsCardsComponent } from './components/statistics-cards/statistics-cards';
import { HallsTableComponent } from '../halls/components/halls-table/halls-table';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [StatisticsCardsComponent, HallsTableComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {}
