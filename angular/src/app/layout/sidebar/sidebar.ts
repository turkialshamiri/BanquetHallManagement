import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { filter, Subscription } from 'rxjs';

import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';

import { LayoutService } from '../services/layout.service';
import { PolicyService } from 'src/app/core/services/policy.service';

interface SidebarChildItem {
  titleKey: string;
  icon: string;
  route: string;
  exact?: boolean;
}

interface SidebarMenuItem {
  sectionId: string;
  titleKey: string;
  icon: string;
  children: SidebarChildItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    MatExpansionModule,
    MatIconModule,
    AppLocalizationPipe,
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

  private readonly sectionPermissions: Record<string, string> = {
    dashboard: 'BanquetHallManagement.Dashboard',
    halls: 'BanquetHallManagement.Halls',
    customers: 'BanquetHallManagement.Customers',
    services: 'BanquetHallManagement.Services',
    reservations: 'BanquetHallManagement.Reservations',
    reports: 'BanquetHallManagement.Reports',
    users: 'BanquetHallManagement.Users',
  };

  private readonly allMenuItems: SidebarMenuItem[] = [
    {
      sectionId: 'dashboard',
      titleKey: 'Menu:Section:Dashboard',
      icon: 'dashboard',
      children: [
        {
          titleKey: 'Menu:Dashboard:Home',
          icon: 'home',
          route: '/dashboard',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'halls',
      titleKey: 'Menu:Section:Halls',
      icon: 'apartment',
      children: [
        {
          titleKey: 'Menu:Halls:All',
          icon: 'apartment',
          route: '/halls/all',
          exact: true,
        },
        {
          titleKey: 'Menu:Halls:Available',
          icon: 'check_circle',
          route: '/halls/available',
          exact: true,
        },
        {
          titleKey: 'Menu:Halls:Booked',
          icon: 'event_busy',
          route: '/halls/booked',
          exact: true,
        },
        {
          titleKey: 'Menu:Halls:Maintenance',
          icon: 'build',
          route: '/halls/maintenance',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'customers',
      titleKey: 'Menu:Section:Customers',
      icon: 'groups',
      children: [
        {
          titleKey: 'Menu:Customers:List',
          icon: 'group',
          route: '/customers',
          exact: true,
        },
        {
          titleKey: 'Menu:Customers:Create',
          icon: 'person_add',
          route: '/customers/create',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'services',
      titleKey: 'Menu:Section:Services',
      icon: 'room_service',
      children: [
        {
          titleKey: 'Menu:Services:List',
          icon: 'list_alt',
          route: '/services',
          exact: true,
        },
        {
          titleKey: 'Menu:Services:Create',
          icon: 'add_circle',
          route: '/services/create',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'reservations',
      titleKey: 'Menu:Section:Reservations',
      icon: 'event_available',
      children: [
        {
          titleKey: 'Menu:Reservations:List',
          icon: 'calendar_month',
          route: '/bookings',
          exact: true,
        },
        {
          titleKey: 'Menu:Reservations:Create',
          icon: 'add_circle',
          route: '/bookings/create',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'reports',
      titleKey: 'Menu:Section:Reports',
      icon: 'analytics',
      children: [
        {
          titleKey: 'Menu:Reports:Analytics',
          icon: 'insights',
          route: '/reports',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'users',
      titleKey: 'Menu:Section:Users',
      icon: 'manage_accounts',
      children: [
        {
          titleKey: 'Menu:Users:Employees',
          icon: 'group_manage',
          route: '/users',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'about',
      titleKey: 'Menu:Section:About',
      icon: 'info',
      children: [
        {
          titleKey: 'Menu:About:System',
          icon: 'info_outline',
          route: '/about',
          exact: true,
        },
      ],
    },
    {
      sectionId: 'support',
      titleKey: 'Menu:Section:Support',
      icon: 'support_agent',
      children: [
        {
          titleKey: 'Menu:Support:Contact',
          icon: 'contact_support',
          route: '/support/contact',
          exact: true,
        },
        {
          titleKey: 'Menu:Support:Ticket',
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
        section.sectionId === 'about' || section.sectionId === 'support'
          ? can('BanquetHallManagement.Halls.Create')
          : can(this.sectionPermissions[section.sectionId] ?? '');

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
