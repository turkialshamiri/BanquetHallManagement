import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';

import { LayoutService } from '../services/layout.service';

interface SidebarChildItem {
  title: string;
  icon: string;
  route: string;
  exact?: boolean;
}

interface SidebarMenuItem {
  title: string;
  icon: string;
  children: SidebarChildItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatExpansionModule,
    MatIconModule,
  ],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.scss'],
})
export class Sidebar implements OnInit {
  private router = inject(Router);
  layoutService = inject(LayoutService);

  menuItems: SidebarMenuItem[] = [
    {
      title: 'لوحة التحكم',
      icon: 'dashboard',
      children: [
        {
          title: 'الرئيسية',
          icon: 'home',
          route: '/dashboard',
          exact: true,
        },
      ],
    },
    {
      title: 'القاعات',
      icon: 'apartment',
      children: [
        {
          title: 'جميع القاعات',
          icon: 'apartment',
          route: '/halls/all',
          exact: true,
        },
        {
          title: 'القاعات المتاحة',
          icon: 'check_circle',
          route: '/halls/available',
          exact: true,
        },
        {
          title: 'القاعات المحجوزة',
          icon: 'event_busy',
          route: '/halls/booked',
          exact: true,
        },
        {
          title: 'تحت الصيانة',
          icon: 'build',
          route: '/halls/maintenance',
          exact: true,
        },
      ],
    },
    {
      title: 'العملاء',
      icon: 'groups',
      children: [
        {
          title: 'عرض العملاء',
          icon: 'group',
          route: '/customers',
          exact: true,
        },
        {
          title: 'إضافة عميل',
          icon: 'person_add',
          route: '/customers/create',
          exact: true,
        },
      ],
    },
    {
      title: 'الخدمات',
      icon: 'room_service',
      children: [
        {
          title: 'عرض الخدمات',
          icon: 'list_alt',
          route: '/services',
          exact: true,
        },
        {
          title: 'إضافة خدمة',
          icon: 'add_circle',
          route: '/services/create',
          exact: true,
        },
      ],
    },
    {
      title: 'الحجوزات',
      icon: 'event_available',
      children: [
        {
          title: 'عرض الحجوزات',
          icon: 'calendar_month',
          route: '/bookings',
          exact: true,
        },
        {
          title: 'إضافة حجز',
          icon: 'add_circle',
          route: '/bookings/create',
          exact: true,
        },
      ],
    },
    {
      title: 'التقارير',
      icon: 'analytics',
      children: [
        {
          title: 'التقارير والتحليلات',
          icon: 'insights',
          route: '/reports',
          exact: true,
        },
      ],
    },
    {
      title: 'من نحن',
      icon: 'info',
      children: [
        {
          title: 'التعريف بالنظام',
          icon: 'info_outline',
          route: '/about',
          exact: true,
        },
      ],
    },
    {
      title: 'الدعم الفني',
      icon: 'support_agent',
      children: [
        {
          title: 'تواصل معنا',
          icon: 'contact_support',
          route: '/support/contact',
          exact: true,
        },
        {
          title: 'إرسال شكوى',
          icon: 'report_problem',
          route: '/support/ticket',
          exact: true,
        },
      ],
    },
  ];

  ngOnInit(): void {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => {
        if (this.layoutService.isMobile()) {
          this.layoutService.closeSidebar();
        }
      });
  }

  get sidenavMode(): 'over' | 'side' {
    return this.layoutService.isMobile() ? 'over' : 'side';
  }

  get sidenavOpened(): boolean {
    return this.layoutService.isMobile()
      ? this.layoutService.sidebarOpen()
      : true;
  }

  isSectionActive(item: SidebarMenuItem): boolean {
    const currentUrl = this.router.url.split('?')[0];

    return item.children.some((child) => this.isRouteActive(child.route, currentUrl));
  }

  isRouteActive(route: string, currentUrl: string = this.router.url.split('?')[0]): boolean {
    return currentUrl === route;
  }

  onNavClick(): void {
    if (this.layoutService.isMobile()) {
      this.layoutService.closeSidebar();
    }
  }
}

