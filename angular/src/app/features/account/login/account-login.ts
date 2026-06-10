import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '@abp/ng.core';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';

@Component({
  selector: 'app-account-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AppLocalizationPipe],
  templateUrl: './account-login.html',
  styleUrl: './account-login.scss',
})
export class AccountLogin {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private l10n = inject(AppLocalizationService);

  loading = false;
  errorMessage: string | null = null;

  form = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
    rememberMe: [true],
  });

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const query = this.route.snapshot.queryParamMap;
    const redirectUrl =
      query.get('redirectUrl') ?? query.get('returnUrl') ?? '/dashboard';

    const { username, password, rememberMe } = this.form.getRawValue();

    this.authService
      .login({ username, password, rememberMe, redirectUrl })
      .subscribe({
        next: () => {
          this.loading = false;
        },
        error: (err) => {
          this.loading = false;

          this.errorMessage =
            err?.error?.error_description ??
            getAbpErrorMessage(err) ??
            this.l10n.instant('Login:Failed');
        },
      });
  }
}
