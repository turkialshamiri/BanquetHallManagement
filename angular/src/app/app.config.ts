import { ApplicationConfig, importProvidersFrom, inject, provideAppInitializer } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { provideAbpCore, withOptions } from '@abp/ng.core';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { registerLocaleForEsBuild } from '@abp/ng.core/locale';

import { environment } from '../environments/environment';
import { APP_ROUTES } from './app.routes';
import { LanguageDirectionService } from './core/services/language-direction.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(APP_ROUTES, withEnabledBlockingInitialNavigation()),
    provideAnimations(),
    importProvidersFrom(MatSnackBarModule),

    provideAbpCore(
      withOptions({
        environment,
        registerLocaleFn: registerLocaleForEsBuild(),
      })
    ),

    provideAbpOAuth(),

    provideAppInitializer(() => {
      inject(LanguageDirectionService).init();
    }),
  ],
};