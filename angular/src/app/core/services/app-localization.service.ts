import { Injectable, inject } from '@angular/core';
import { LocalizationService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { toAbpL10nKey } from '../localization/l10n-key.util';

@Injectable({ providedIn: 'root' })
export class AppLocalizationService {
  private readonly l10n = inject(LocalizationService);

  instant(key: string, ...interpolateParams: string[]): string {
    return this.l10n.instant(toAbpL10nKey(key), ...interpolateParams);
  }

  get(key: string, ...interpolateParams: string[]): Observable<string> {
    return this.l10n.get(toAbpL10nKey(key), ...interpolateParams);
  }
}
