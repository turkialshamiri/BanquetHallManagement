import { Component } from '@angular/core';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { RouterOutlet } from '@angular/router';
import { StatisticsCardsComponent } from './components/statistics-cards/statistics-cards';
import { HallsTableComponent } from './components/halls-table/halls-table';

@Component({
  selector: 'app-dashboard',
  imports: [Sidebar, Navbar,StatisticsCardsComponent, HallsTableComponent, RouterOutlet],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  halls = [
  {
    id: '1',
    name: 'قاعة الامراء',
    description: 'قاعة مجهزة باحدث الديكورات واللمسات',
    capacity: 300,
    location: 'هجدة',
    pricePerHour: 100000,
    status: 1,
    type: 1,
    creationTime: '2026-06-02'
  }
];
}
