import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';

import { AuthService, ConfigStateService } from '@abp/ng.core';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { LayoutService } from '../services/layout.service';
import {
  AppLanguage,
  LanguageDirectionService,
  SUPPORTED_LANGUAGES,
} from 'src/app/core/services/language-direction.service';

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
    AppLocalizationPipe,
  ],
})
export class Navbar {
  layoutService = inject(LayoutService);
  private configState = inject(ConfigStateService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private languageDirection = inject(LanguageDirectionService);

  currentUser$ = this.configState.getOne$('currentUser');
  readonly languages: AppLanguage[] = SUPPORTED_LANGUAGES;

  get currentLanguage(): string {
    return this.languageDirection.getCurrentLanguage();
  }

  get currentLanguageLabelKey(): string {
    return (
      this.languages.find((lang) => lang.cultureName === this.currentLanguage)
        ?.displayNameKey ?? 'Language'
    );
  }

  isCurrentLanguage(cultureName: string): boolean {
    return this.currentLanguage === cultureName;
  }

  switchLanguage(cultureName: string): void {
    this.languageDirection.switchLanguage(cultureName);
  }

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
