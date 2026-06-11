import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

const hallsPage = () =>
  import('./features/halls/halls').then((m) => m.Halls);

const hallsRouteDefaults = {
  pageTitleKey: 'Halls:Title',
  pageSubtitleKey: 'Halls:Subtitle',
  statusFilter: null,
};

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'account/login',
  },
  {
    path: 'login',
    redirectTo: 'account/login',
    pathMatch: 'full',
  },
  {
    path: 'account/login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/account/login/account-login').then((m) => m.AccountLogin),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
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
          pageTitleKey: 'Halls:Available:Title',
          pageSubtitleKey: 'Halls:Available:Subtitle',
          statusFilter: 1,
        },
      },
      {
        path: 'halls/booked',
        loadComponent: hallsPage,
        data: {
          pageTitleKey: 'Halls:Booked:Title',
          pageSubtitleKey: 'Halls:Booked:Subtitle',
          statusFilter: 3,
        },
      },
      {
        path: 'halls/maintenance',
        loadComponent: hallsPage,
        data: {
          pageTitleKey: 'Halls:Maintenance:Title',
          pageSubtitleKey: 'Halls:Maintenance:Subtitle',
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
        path: 'finance/invoices',
        canActivate: [
          permissionGuard('BanquetHallManagement.Finance.Invoices.View'),
        ],
        loadComponent: () =>
          import('./features/finance/invoices/invoices').then((m) => m.Invoices),
      },
      {
        path: 'finance/invoices/:id',
        canActivate: [
          permissionGuard('BanquetHallManagement.Finance.Invoices.View'),
        ],
        loadComponent: () =>
          import('./features/finance/invoices/invoice-detail/invoice-detail').then(
            (m) => m.InvoiceDetail
          ),
      },
      {
        path: 'finance/journal-entries',
        canActivate: [
          permissionGuard('BanquetHallManagement.Finance.ViewJournalEntries'),
        ],
        loadComponent: () =>
          import('./features/finance/journal-entries/journal-entries').then(
            (m) => m.JournalEntries
          ),
      },
      {
        path: 'finance/journal-entries/:id',
        canActivate: [
          permissionGuard('BanquetHallManagement.Finance.ViewJournalEntries'),
        ],
        loadComponent: () =>
          import(
            './features/finance/journal-entries/journal-entry-detail/journal-entry-detail'
          ).then((m) => m.JournalEntryDetail),
      },
      {
        path: 'finance/access-cards/:id',
        canActivate: [
          permissionGuard('BanquetHallManagement.Finance.HallAccessCards.View'),
        ],
        loadComponent: () =>
          import('./features/finance/access-cards/access-card').then(
            (m) => m.AccessCard
          ),
      },
      {
        path: 'finance/refunds',
        canActivate: [
          permissionGuard('BanquetHallManagement.Finance.Refunds.View'),
        ],
        loadComponent: () =>
          import('./features/finance/refunds/refunds').then((m) => m.Refunds),
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
          pageTitleKey: 'Support:Contact:Title',
          pageDescriptionKey: 'Support:Contact:Description',
        },
      },
      {
        path: 'support/ticket',
        loadComponent: () =>
          import('./features/support/support').then((m) => m.Support),
        data: {
          page: 'ticket',
          pageTitleKey: 'Support:Ticket:Title',
          pageDescriptionKey: 'Support:Ticket:Description',
        },
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'account/login',
  },
];
