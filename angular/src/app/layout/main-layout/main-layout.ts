import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { Navbar } from '../navbar/navbar';
import { Sidebar } from '../sidebar/sidebar';
import { LayoutService } from '../services/layout.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, AppLocalizationPipe, Sidebar, Navbar],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout {
  readonly layoutService = inject(LayoutService);
}
