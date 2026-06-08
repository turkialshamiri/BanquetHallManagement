import { Routes } from '@angular/router';
import { asyncAbpOAuthGuard } from '@abp/ng.oauth';
import { permissionGuard } from './core/guards/permission.guard';

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
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'login',
    redirectTo: 'account/login',
    pathMatch: 'full',
  },
  {
    path: 'account/login',
    loadComponent: () =>
      import('./features/account/login/account-login').then((m) => m.AccountLogin),
  },
  {
    path: '',
    canActivate: [asyncAbpOAuthGuard],
    loadComponent: () =>
      import('./layout/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
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
          pageSubtitle: 'القاعات ذات حجوزات مؤكدة نشطة',
          statusFilter: 3,
        },
      },
      {
        path: 'halls/maintenance',
        loadComponent: hallsPage,
        data: {
          pageTitle: 'قاعات تحت الصيانة',
          pageSubtitle: 'القاعات غير المتاحة مؤقتاً',
          statusFilter: 2,
        },
      },
      {
        path: 'halls/occupied',
        redirectTo: 'halls/booked',
        pathMatch: 'full',
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
        canActivate: [permissionGuard('BanquetHallManagement.Services')],
        loadComponent: () =>
          import('./features/services/services').then((m) => m.Services),
      },
      {
        path: 'services/create',
        canActivate: [permissionGuard('BanquetHallManagement.Services.Create')],
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
        path: 'reports',
        canActivate: [permissionGuard('BanquetHallManagement.Reports')],
        loadComponent: () =>
          import('./features/reports/reports').then((m) => m.Reports),
      },
      {
        path: 'reports/daily',
        redirectTo: 'reports',
        pathMatch: 'full',
      },
      {
        path: 'reports/monthly',
        redirectTo: 'reports',
        pathMatch: 'full',
      },
      {
        path: 'reports/yearly',
        redirectTo: 'reports',
        pathMatch: 'full',
      },
      {
        path: 'reports/revenue',
        redirectTo: 'reports',
        pathMatch: 'full',
      },
      {
        path: 'users',
        canActivate: [permissionGuard('BanquetHallManagement.Users')],
        loadComponent: () =>
          import('./features/users/users').then((m) => m.Users),
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
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
