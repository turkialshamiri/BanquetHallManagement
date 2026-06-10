import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [AppLocalizationPipe],
  templateUrl: './support.html',
  styleUrl: './support.scss',
})
export class Support implements OnInit {
  private route = inject(ActivatedRoute);

  pageTitleKey = 'Support:Contact:Title';
  pageDescriptionKey = 'Support:Contact:Description';
  isTicketPage = false;

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.pageTitleKey =
        (data['pageTitleKey'] as string) ?? this.pageTitleKey;
      this.pageDescriptionKey =
        (data['pageDescriptionKey'] as string) ?? this.pageDescriptionKey;
      this.isTicketPage = data['page'] === 'ticket';
    });
  }
}
