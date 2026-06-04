import { Component } from '@angular/core';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { RouterOutlet } from '@angular/router';
import { StatisticsCardsComponent } from './components/statistics-cards/statistics-cards';

@Component({
  selector: 'app-dashboard',
  imports: [Sidebar, Navbar,StatisticsCardsComponent, RouterOutlet],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {

}
