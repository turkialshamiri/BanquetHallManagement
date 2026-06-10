import { DOCUMENT } from '@angular/common';
import {
  Injectable,
  inject,
} from '@angular/core';
import {
  ConfigStateService,
  SessionStateService,
} from '@abp/ng.core';
import {
  BehaviorSubject,
  Observable,
  distinctUntilChanged,
  filter,
  map,
} from 'rxjs';

export type TextDirection = 'rtl' | 'ltr';

export interface AppLanguage {
  cultureName: string;
  displayNameKey: string;
}

export const SUPPORTED_LANGUAGES: AppLanguage[] = [
  { cultureName: 'ar', displayNameKey: 'Language:Arabic' },
  { cultureName: 'en', displayNameKey: 'Language:English' },
];

const DEFAULT_LANGUAGE = 'ar';
const RTL_CULTURES = new Set(['ar', 'ar-SA', 'ar-EG', 'ar-YE']);

@Injectable({ providedIn: 'root' })
export class LanguageDirectionService {
  private readonly document = inject(DOCUMENT);
  private readonly sessionState = inject(SessionStateService);
  private readonly configState = inject(ConfigStateService);

  private readonly directionSubject = new BehaviorSubject<TextDirection>(
    this.resolveDirection(this.getStoredLanguage())
  );

  readonly direction$: Observable<TextDirection> =
    this.directionSubject.asObservable();

  readonly isRtl$: Observable<boolean> = this.direction$.pipe(
    map((direction) => direction === 'rtl')
  );

  readonly currentLanguage$: Observable<string> =
    this.configState.getDeep$('localization.currentCulture.cultureName').pipe(
      map((culture) => culture ?? this.getStoredLanguage()),
      distinctUntilChanged()
    );

  init(): void {
    const culture = this.getStoredLanguage();
    this.applyDocumentDirection(culture);

    this.configState
      .getDeep$('localization.currentCulture.cultureName')
      .pipe(
        filter((culture): culture is string => !!culture),
        distinctUntilChanged()
      )
      .subscribe((culture) => this.applyDocumentDirection(culture));
  }

  getCurrentLanguage(): string {
    return (
      this.configState.getDeep('localization.currentCulture.cultureName') ??
      this.getStoredLanguage()
    );
  }

  getCurrentDirection(): TextDirection {
    return this.directionSubject.value;
  }

  switchLanguage(cultureName: string): void {
    if (cultureName === this.getCurrentLanguage()) {
      return;
    }

    this.sessionState.setLanguage(cultureName);
    this.applyDocumentDirection(cultureName);
  }

  private applyDocumentDirection(cultureName: string): void {
    const direction = this.resolveDirection(cultureName);
    const html = this.document.documentElement;
    const body = this.document.body;

    html.lang = cultureName;
    html.dir = direction;
    body.dir = direction;
    body.classList.remove('app-rtl', 'app-ltr');
    body.classList.add(direction === 'rtl' ? 'app-rtl' : 'app-ltr');

    this.directionSubject.next(direction);
  }

  private getStoredLanguage(): string {
    return this.sessionState.getLanguage() ?? DEFAULT_LANGUAGE;
  }

  private resolveDirection(cultureName: string): TextDirection {
    const normalized = cultureName.toLowerCase();
    return RTL_CULTURES.has(normalized) || normalized.startsWith('ar')
      ? 'rtl'
      : 'ltr';
  }
}
