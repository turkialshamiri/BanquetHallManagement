import { Component } from '@angular/core';
import { HallsTableComponent } from './components/halls-table/halls-table';

@Component({
  selector: 'app-halls',
  standalone: true,
  imports: [HallsTableComponent],
  templateUrl: './halls.html',
  styleUrl: './halls.scss',
})
export class Halls {}
