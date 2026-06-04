import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-sidebar',
  standalone: true,

  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatExpansionModule,
    MatIconModule
  ],

  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.scss']
})
export class Sidebar {

  menuItems = [

    {
      title: 'القاعات',
      icon: 'apartment',
      children: [
        {
          title: 'جميع القاعات',
          icon: 'apartment',
          route: '/halls/all'
        },
        {
          title: 'القاعات المتاحة',
          icon: 'check_circle',
          route: '/halls/available'
        },
        {
          title: 'القاعات المحجوزة',
          icon: 'event_busy',
          route: '/halls/booked'
        },
        {
          title: 'القاعات المشغولة',
          icon: 'meeting_room',
          route: '/halls/occupied'
        },
        {
          title: 'تحت الصيانة',
          icon: 'build',
          route: '/halls/maintenance'
        }
      ]
    },

    {
      title: 'العملاء',
      icon: 'groups',
      children: [
        {
          title: 'عرض العملاء',
          icon: 'group',
          route: '/customers'
        },
        {
          title: 'إضافة عميل',
          icon: 'person_add',
          route: '/customers/create'
        }
      ]
    },

    {
      title: 'الحجوزات',
      icon: 'event_available',
      children: [
        {
          title: 'عرض الحجوزات',
          icon: 'calendar_month',
          route: '/bookings'
        },
        {
          title: 'إضافة حجز',
          icon: 'add_circle',
          route: '/bookings/create'
        }
      ]
    },

    {
      title: 'التقارير',
      icon: 'analytics',
      children: [
        {
          title: 'التقارير اليومية',
          icon: 'today',
          route: '/reports/daily'
        },
        {
          title: 'التقارير الشهرية',
          icon: 'date_range',
          route: '/reports/monthly'
        },
        {
          title: 'التقارير السنوية',
          icon: 'assessment',
          route: '/reports/yearly'
        },
        {
          title: 'تقارير الإيرادات',
          icon: 'payments',
          route: '/reports/revenue'
        }
      ]
    },

    {
      title: 'من نحن',
      icon: 'info',
      children: [
        {
          title: 'التعريف بالنظام',
          icon: 'info_outline',
          route: '/about'
        }
      ]
    },

    {
      title: 'الدعم الفني',
      icon: 'support_agent',
      children: [
        {
          title: 'تواصل معنا',
          icon: 'contact_support',
          route: '/support/contact'
        },
        {
          title: 'إرسال شكوى',
          icon: 'report_problem',
          route: '/support/ticket'
        }
      ]
    }

  ];

}