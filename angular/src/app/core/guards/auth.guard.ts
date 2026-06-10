import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';

function hasValidSession(): boolean {
  return inject(OAuthService).hasValidAccessToken();
}

/**
 * Protects authenticated application routes.
 * Returns a UrlTree to the login page instead of imperative navigation.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);

  if (hasValidSession()) {
    return true;
  }

  return router.createUrlTree(['/account/login'], {
    queryParams: { returnUrl: state.url },
  });
};

/**
 * Prevents authenticated users from accessing the login page.
 */
export const guestGuard: CanActivateFn = () => {
  const router = inject(Router);

  return hasValidSession() ? router.createUrlTree(['/dashboard']) : true;
};
