import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { filter, Subscription } from 'rxjs';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';

import { LayoutService } from '../services/layout.service';
import { PolicyService } from 'src/app/core/services/policy.service';

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
    MatSidenavModule,
    MatExpansionModule,
    MatIconModule,
  ],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.scss'],
})
export class Sidebar implements OnInit, OnDestroy {
  private router = inject(Router);
  layoutService = inject(LayoutService);
  private policy = inject(PolicyService);
  private configState = inject(ConfigStateService);
  private authSubscription?: Subscription;

  menuItems: SidebarMenuItem[] = [];

  private readonly allMenuItems: SidebarMenuItem[] = [
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
      title: 'المستخدمون',
      icon: 'manage_accounts',
      children: [
        {
          title: 'إدارة الموظفين',
          icon: 'group_manage',
          route: '/users',
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
    this.refreshMenuItems();

    this.authSubscription = this.configState.getOne$('auth').subscribe(() => {
      this.refreshMenuItems();
    });

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => {
        if (this.layoutService.isMobile()) {
          this.layoutService.closeSidebar();
        }
      });
  }

  ngOnDestroy(): void {
    this.authSubscription?.unsubscribe();
  }

  private refreshMenuItems(): void {
    const can = (policy: string) => this.policy.hasSnapshot(policy);
    const filtered: SidebarMenuItem[] = [];

    for (const section of this.allMenuItems) {
      const sectionVisible =
        section.title === 'لوحة التحكم'
          ? can('BanquetHallManagement.Dashboard')
          : section.title === 'القاعات'
          ? can('BanquetHallManagement.Halls')
          : section.title === 'العملاء'
          ? can('BanquetHallManagement.Customers')
          : section.title === 'الخدمات'
          ? can('BanquetHallManagement.Services')
          : section.title === 'الحجوزات'
          ? can('BanquetHallManagement.Reservations')
          : section.title === 'التقارير'
          ? can('BanquetHallManagement.Reports')
          : section.title === 'المستخدمون'
          ? can('BanquetHallManagement.Users')
          : section.title === 'من نحن' || section.title === 'الدعم الفني'
          ? can('BanquetHallManagement.Halls.Create')
          : false;

      if (!sectionVisible) {
        continue;
      }

      const children = section.children.filter((child) => {
        if (child.route === '/customers/create') {
          return can('BanquetHallManagement.Customers.Create');
        }
        if (child.route === '/services/create') {
          return can('BanquetHallManagement.Services.Create');
        }
        if (child.route === '/bookings/create') {
          return can('BanquetHallManagement.Reservations.Create');
        }
        return true;
      });

      if (children.length === 0) {
        continue;
      }

      filtered.push({ ...section, children });
    }

    this.menuItems = filtered;
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

  navigateTo(route: string): void {
    if (this.router.url.split('?')[0] !== route) {
      void this.router.navigateByUrl(route);
    }

    if (this.layoutService.isMobile()) {
      this.layoutService.closeSidebar();
    }
  }
}

