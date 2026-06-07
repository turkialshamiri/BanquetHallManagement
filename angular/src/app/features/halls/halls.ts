import { Component } from '@angular/core';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { HallsTableComponent } from './components/halls-table/halls-table';

@Component({
  selector: 'app-halls',
  standalone: true,
  imports: [Sidebar, Navbar, HallsTableComponent],
  templateUrl: './halls.html',
  styleUrl: './halls.scss',
})
export class Halls {}
