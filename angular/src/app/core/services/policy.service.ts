import { Injectable, inject } from '@angular/core';
import { ConfigStateService } from '@abp/ng.core';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class PolicyService {
  private configState = inject(ConfigStateService);

  has$(policyName: string) {
    return this.configState.getOne$('auth').pipe(
      map((auth) => auth?.grantedPolicies?.[policyName] === true)
    );
  }

  hasSnapshot(policyName: string): boolean {
    const state = this.configState.getAll();
    return state?.auth?.grantedPolicies?.[policyName] === true;
  }
}

