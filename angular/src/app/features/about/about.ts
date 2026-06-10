import { Component } from '@angular/core';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [AppLocalizationPipe],
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class About {}
