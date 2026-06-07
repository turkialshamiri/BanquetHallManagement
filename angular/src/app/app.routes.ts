import { Routes } from '@angular/router';

const hallsPage = () =>
  import('./features/halls/halls').then((m) => m.Halls);

const hallsRouteDefaults = {
  pageTitle: 'القاعات',
  pageSubtitle: 'جميع القاعات المسجلة بالنظام',
  statusFilter: null,
};

export const APP_ROUTES: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard').then((m) => m.Dashboard),
  },
  {
    path: 'halls/all',
    loadComponent: hallsPage,
    data: hallsRouteDefaults,
  },
  {
    path: 'halls/available',
    loadComponent: hallsPage,
    data: {
      pageTitle: 'القاعات المتاحة',
      pageSubtitle: 'القاعات الجاهزة للحجز',
      statusFilter: 1,
    },
  },
  {
    path: 'halls/booked',
    loadComponent: hallsPage,
    data: {
      pageTitle: 'القاعات المحجوزة',
      pageSubtitle: 'القاعات التي تم حجزها',
      statusFilter: 2,
    },
  },
  {
    path: 'halls/occupied',
    loadComponent: hallsPage,
    data: {
      pageTitle: 'القاعات المشغولة',
      pageSubtitle: 'القاعات قيد الاستخدام',
      statusFilter: 3,
    },
  },
  {
    path: 'halls/maintenance',
    loadComponent: hallsPage,
    data: {
      pageTitle: 'قاعات تحت الصيانة',
      pageSubtitle: 'القاعات غير المتاحة مؤقتاً',
      statusFilter: 4,
    },
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./features/customers/customers').then((m) => m.Customers),
  },
  {
    path: 'customers/create',
    loadComponent: () =>
      import('./features/customers/customers').then((m) => m.Customers),
    data: { openAddDialog: true },
  },
  {
    path: 'services',
    loadComponent: () =>
      import('./features/services/services').then((m) => m.Services),
  },
  {
    path: 'services/create',
    loadComponent: () =>
      import('./features/services/services').then((m) => m.Services),
    data: { openAddDialog: true },
  },
  {
    path: 'bookings',
    loadComponent: () =>
      import('./features/bookings/bookings').then((m) => m.Bookings),
  },
  {
    path: 'bookings/create',
    loadComponent: () =>
      import('./features/bookings/bookings').then((m) => m.Bookings),
    data: { openAddDialog: true },
  },
  {
    path: 'reports/daily',
    loadComponent: () =>
      import('./features/reports/reports').then((m) => m.Reports),
    data: {
      pageTitle: 'التقارير اليومية',
      pageDescription: 'ملخص العمليات والحجوزات لليوم الحالي',
    },
  },
  {
    path: 'reports/monthly',
    loadComponent: () =>
      import('./features/reports/reports').then((m) => m.Reports),
    data: {
      pageTitle: 'التقارير الشهرية',
      pageDescription: 'تحليل الأداء والحجوزات خلال الشهر',
    },
  },
  {
    path: 'reports/yearly',
    loadComponent: () =>
      import('./features/reports/reports').then((m) => m.Reports),
    data: {
      pageTitle: 'التقارير السنوية',
      pageDescription: 'نظرة شاملة على أداء النظام خلال السنة',
    },
  },
  {
    path: 'reports/revenue',
    loadComponent: () =>
      import('./features/reports/reports').then((m) => m.Reports),
    data: {
      pageTitle: 'تقارير الإيرادات',
      pageDescription: 'متابعة الإيرادات والتسعير',
    },
  },
  {
    path: 'about',
    loadComponent: () =>
      import('./features/about/about').then((m) => m.About),
  },
  {
    path: 'support/contact',
    loadComponent: () =>
      import('./features/support/support').then((m) => m.Support),
    data: {
      page: 'contact',
      pageTitle: 'تواصل معنا',
      pageDescription: 'طرق التواصل مع فريق الدعم الفني',
    },
  },
  {
    path: 'support/ticket',
    loadComponent: () =>
      import('./features/support/support').then((m) => m.Support),
    data: {
      page: 'ticket',
      pageTitle: 'إرسال شكوى',
      pageDescription: 'تسجيل شكوى أو بلاغ للمتابعة',
    },
  },
  {
    path: '**',
    redirectTo: '/dashboard',
  },
];
