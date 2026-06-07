
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard')
        .then(m => m.Dashboard),
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./features/customers/customers')
        .then(m => m.Customers),
  },
  {
    path: 'customers/create',
    loadComponent: () =>
      import('./features/customers/customers')
        .then(m => m.Customers),
    data: { openAddDialog: true },
  },
];
