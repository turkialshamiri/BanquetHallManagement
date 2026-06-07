import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [Sidebar, Navbar],
  templateUrl: './support.html',
  styleUrl: './support.scss',
})
export class Support implements OnInit {
  private route = inject(ActivatedRoute);

  pageTitle = 'الدعم الفني';
  pageDescription = 'تواصل مع فريق الدعم';
  isTicketPage = false;

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.pageTitle = (data['pageTitle'] as string) ?? this.pageTitle;
      this.pageDescription =
        (data['pageDescription'] as string) ?? this.pageDescription;
      this.isTicketPage = data['page'] === 'ticket';
    });
  }
}
