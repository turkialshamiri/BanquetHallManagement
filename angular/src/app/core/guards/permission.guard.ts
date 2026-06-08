import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { combineLatest } from 'rxjs';
import { filter, map, take } from 'rxjs/operators';
import { PolicyService } from '../services/policy.service';

/**
 * Route guard for ABP permission policies.
 *
 * Policy names contain dots (e.g. "BanquetHallManagement.Reports") and must be
 * read from auth.grantedPolicies via the full key — not via getDeep(), which
 * treats dots as nested path segments.
 */
export function permissionGuard(requiredPolicy: string): CanActivateFn {
  return () => {
    const configState = inject(ConfigStateService);
    const policy = inject(PolicyService);
    const router = inject(Router);

    return combineLatest([
      configState.getOne$('currentUser'),
      configState.getOne$('auth'),
    ]).pipe(
      filter(
        ([user, auth]) =>
          user?.isAuthenticated === true && auth?.grantedPolicies != null
      ),
      take(1),
      map(() =>
        policy.hasSnapshot(requiredPolicy)
          ? true
          : router.createUrlTree(['/dashboard'])
      )
    );
  };
}
