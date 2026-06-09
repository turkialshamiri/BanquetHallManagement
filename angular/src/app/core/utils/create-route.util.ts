import { Router } from '@angular/router';

/** Navigate from a /…/create shortcut route back to the list route after the dialog closes. */
export function leaveCreateRoute(router: Router, createPath: string, listPath: string): void {
  if (router.url.includes(createPath)) {
    void router.navigate([listPath], { replaceUrl: true });
  }
}
