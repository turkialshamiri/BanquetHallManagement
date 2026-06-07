import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [Sidebar, Navbar],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports implements OnInit {
  private route = inject(ActivatedRoute);

  pageTitle = 'التقارير';
  pageDescription = 'عرض التقارير والإحصائيات';

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.pageTitle = (data['pageTitle'] as string) ?? this.pageTitle;
      this.pageDescription =
        (data['pageDescription'] as string) ?? this.pageDescription;
    });
  }
}
