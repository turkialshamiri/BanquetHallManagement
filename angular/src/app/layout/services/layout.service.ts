import { BreakpointObserver } from '@angular/cdk/layout';
import { Injectable, inject, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  private breakpointObserver = inject(BreakpointObserver);

  readonly sidebarOpen = signal(false);
  readonly isMobile = signal(false);

  constructor() {
    this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe((state) => {
        this.isMobile.set(state.matches);

        if (!state.matches) {
          this.sidebarOpen.set(false);
        }
      });
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((open) => !open);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }
}
