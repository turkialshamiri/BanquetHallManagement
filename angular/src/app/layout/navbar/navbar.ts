import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';

import { LayoutService } from '../services/layout.service';
import { AuthService, ConfigStateService } from '@abp/ng.core';

@Component({
  selector: 'app-navbar',
  standalone: true,
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss'],
  imports: [
    CommonModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule,
  ],
})
export class Navbar {
  layoutService = inject(LayoutService);
  private configState = inject(ConfigStateService);
  private authService = inject(AuthService);
  private router = inject(Router);

  currentUser$ = this.configState.getOne$('currentUser');

  logout(): void {
    this.authService.logout().subscribe({
      next: () => this.navigateToLogin(),
      error: () => this.navigateToLogin(),
    });
  }

  private navigateToLogin(): void {
    void this.router.navigateByUrl('/account/login', { replaceUrl: true });
  }
}
