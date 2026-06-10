import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { OAuthService } from 'angular-oauth2-oidc';
import { combineLatest, of } from 'rxjs';
import { catchError, filter, map, take, timeout } from 'rxjs/operators';

const SESSION_READY_TIMEOUT_MS = 8000;

/**
 * Waits for ABP app state, then requires a valid token AND authenticated user.
 */
function isAuthenticatedWhenReady$() {
  const configState = inject(ConfigStateService);
  const oAuth = inject(OAuthService);

  return combineLatest([
    configState.getOne$('auth'),
    configState.getOne$('currentUser'),
  ]).pipe(
    filter(([auth]) => auth != null),
    take(1),
    map(([, user]) => oAuth.hasValidAccessToken() && user?.isAuthenticated === true),
    timeout(SESSION_READY_TIMEOUT_MS),
    catchError(() => of(false))
  );
}

/**
 * Protects authenticated application routes.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);

  return isAuthenticatedWhenReady$().pipe(
    map((ok) =>
      ok
        ? true
        : router.createUrlTree(['/account/login'], {
            queryParams: { returnUrl: state.url },
          })
    )
  );
};

/**
 * Prevents authenticated users from accessing the login page.
 */
export const guestGuard: CanActivateFn = () => {
  const router = inject(Router);

  return isAuthenticatedWhenReady$().pipe(
    map((ok) => (ok ? router.createUrlTree(['/dashboard']) : true))
  );
};
