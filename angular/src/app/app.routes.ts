
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
];
