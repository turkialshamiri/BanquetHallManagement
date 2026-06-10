import {
  ChangeDetectorRef,
  OnDestroy,
  Pipe,
  PipeTransform,
  inject,
} from '@angular/core';
import { LocalizationService } from '@abp/ng.core';
import { Subscription, merge } from 'rxjs';
import { toAbpL10nKey } from '../localization/l10n-key.util';

/**
 * Drop-in replacement for abpLocalization that:
 * 1. Prefixes keys with :: (default resource) per ABP format
 * 2. Re-renders when language / localization resources change (impure)
 */
@Pipe({
  name: 'appLocalization',
  standalone: true,
  pure: false,
})
export class AppLocalizationPipe implements PipeTransform, OnDestroy {
  private readonly localization = inject(LocalizationService);
  private readonly cdr = inject(ChangeDetectorRef);
  private subscription?: Subscription;

  constructor() {
    this.subscription = merge(
      this.localization.languageChange$,
      this.localization.currentLang$
    ).subscribe(() => this.cdr.markForCheck());
  }

  transform(value = '', ...interpolateParams: unknown[]): string {
    const params = interpolateParams.reduce<unknown[]>((acc, val) => {
      if (!acc.length) {
        return Array.isArray(val) ? [...val] : [val];
      }
      if (!val) {
        return acc;
      }
      return Array.isArray(val) ? [...acc, ...val] : [...acc, val];
    }, []);

    return this.localization.instant(
      toAbpL10nKey(value),
      ...(params as string[])
    );
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
