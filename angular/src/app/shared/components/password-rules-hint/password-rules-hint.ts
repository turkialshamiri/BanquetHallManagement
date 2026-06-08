import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { evaluatePasswordRules } from 'src/app/core/validators/identity-password.validator';

@Component({
  selector: 'app-password-rules-hint',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './password-rules-hint.html',
  styleUrl: './password-rules-hint.scss',
})
export class PasswordRulesHint {
  @Input() password = '';
  @Input() showWhenEmpty = false;

  get rules() {
    return evaluatePasswordRules(this.password);
  }

  get visible(): boolean {
    return this.showWhenEmpty || this.password.length > 0;
  }
}
