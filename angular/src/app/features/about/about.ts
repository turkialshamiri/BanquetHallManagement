import { Component } from '@angular/core';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [Sidebar, Navbar],
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class About {}
