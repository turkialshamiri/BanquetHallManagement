import { Component } from '@angular/core';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [Sidebar, Navbar],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {

}
