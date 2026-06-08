import { getAbpErrorMessage } from './abp-error.util';

const PASSWORD_ERROR_MAP: Array<{ pattern: RegExp; message: string }> = [
  {
    pattern: /uppercase|capital/i,
    message: 'يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل',
  },
  {
    pattern: /lowercase/i,
    message: 'يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل',
  },
  {
    pattern: /digit|number|رقم/i,
    message: 'يجب أن تحتوي كلمة المرور على رقم واحد على الأقل',
  },
  {
    pattern: /non.?alphanumeric|special|رمز/i,
    message: 'يجب أن تحتوي كلمة المرور على رمز خاص واحد على الأقل',
  },
  {
    pattern: /at least \d+ characters|too short|الحد الأدنى/i,
    message: 'كلمة المرور قصيرة جداً',
  },
  {
    pattern: /username.*already|مستخدم مسبقاً/i,
    message: 'اسم المستخدم مستخدم مسبقاً',
  },
  {
    pattern: /email.*already|البريد/i,
    message: 'البريد الإلكتروني مستخدم مسبقاً',
  },
];

export function getFriendlyIdentityErrorMessage(
  error: unknown,
  fallback = 'تعذر تنفيذ العملية. تحقق من البيانات المدخلة.'
): string {
  const raw = getAbpErrorMessage(error, '');

  if (!raw) {
    return fallback;
  }

  for (const entry of PASSWORD_ERROR_MAP) {
    if (entry.pattern.test(raw)) {
      return entry.message;
    }
  }

  if (/password/i.test(raw)) {
    return 'كلمة المرور لا تستوفي متطلبات الأمان';
  }

  return raw;
}
