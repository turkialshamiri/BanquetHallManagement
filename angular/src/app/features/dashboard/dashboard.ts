import { Component } from '@angular/core';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { StatisticsCardsComponent } from './components/statistics-cards/statistics-cards';
import { HallsTableComponent } from '../halls/components/halls-table/halls-table';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [Sidebar, Navbar, StatisticsCardsComponent, HallsTableComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {}
