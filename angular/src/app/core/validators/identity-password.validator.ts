/**
 * Matches ABP / ASP.NET Core Identity default password requirements.
 * Keep in sync with ChangeIdentityPasswordPolicySettingDefinitionProvider.
 */
export const IDENTITY_PASSWORD_POLICY = {
  minLength: 6,
  requireUppercase: true,
  requireLowercase: true,
  requireDigit: true,
  requireNonAlphanumeric: true,
} as const;

const UPPERCASE = /[A-Z]/;
const LOWERCASE = /[a-z]/;
const DIGIT = /\d/;
const NON_ALPHANUMERIC = /[^a-zA-Z0-9]/;

export interface PasswordRuleResult {
  key: string;
  label: string;
  valid: boolean;
}

export function evaluatePasswordRules(password: string): PasswordRuleResult[] {
  const rules: PasswordRuleResult[] = [
    {
      key: 'minLength',
      label: `الحد الأدنى ${IDENTITY_PASSWORD_POLICY.minLength} أحرف`,
      valid: password.length >= IDENTITY_PASSWORD_POLICY.minLength,
    },
  ];

  if (IDENTITY_PASSWORD_POLICY.requireUppercase) {
    rules.push({
      key: 'uppercase',
      label: 'حرف كبير واحد على الأقل',
      valid: UPPERCASE.test(password),
    });
  }

  if (IDENTITY_PASSWORD_POLICY.requireLowercase) {
    rules.push({
      key: 'lowercase',
      label: 'حرف صغير واحد على الأقل',
      valid: LOWERCASE.test(password),
    });
  }

  if (IDENTITY_PASSWORD_POLICY.requireDigit) {
    rules.push({
      key: 'digit',
      label: 'رقم واحد على الأقل',
      valid: DIGIT.test(password),
    });
  }

  if (IDENTITY_PASSWORD_POLICY.requireNonAlphanumeric) {
    rules.push({
      key: 'special',
      label: 'رمز خاص واحد على الأقل',
      valid: NON_ALPHANUMERIC.test(password),
    });
  }

  return rules;
}

export function isIdentityPasswordValid(password: string): boolean {
  return evaluatePasswordRules(password).every((rule) => rule.valid);
}

export function getIdentityPasswordValidationMessage(password: string): string | null {
  const failedRule = evaluatePasswordRules(password).find((rule) => !rule.valid);

  if (!failedRule) {
    return null;
  }

  switch (failedRule.key) {
    case 'minLength':
      return `يجب ألا تقل كلمة المرور عن ${IDENTITY_PASSWORD_POLICY.minLength} أحرف`;
    case 'uppercase':
      return 'يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل';
    case 'lowercase':
      return 'يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل';
    case 'digit':
      return 'يجب أن تحتوي كلمة المرور على رقم واحد على الأقل';
    case 'special':
      return 'يجب أن تحتوي كلمة المرور على رمز خاص واحد على الأقل';
    default:
      return 'كلمة المرور لا تستوفي متطلبات الأمان';
  }
}

export function getConfirmPasswordValidationMessage(
  password: string,
  confirmPassword: string
): string | null {
  if (!confirmPassword) {
    return 'تأكيد كلمة المرور مطلوب';
  }

  if (password !== confirmPassword) {
    return 'كلمتا المرور غير متطابقتين';
  }

  return null;
}
