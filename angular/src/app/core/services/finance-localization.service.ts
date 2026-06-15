import { Injectable, inject } from '@angular/core';
import { AppLocalizationService } from './app-localization.service';

/** Chart-of-accounts codes — must stay aligned with FinanceAccountCodes on the backend. */
const FINANCE_ACCOUNT_CODES = {
  Cash: '1100',
  CustomerRefundLiabilities: '2200',
  DeferredRevenue: '2300',
  HallRevenue: '4100',
  NonRefundableDepositRevenue: '4110',
  ServiceRevenue: '4200',
} as const;

/** English account names persisted in the database (display lookup fallback). */
const ACCOUNT_NAME_TO_CODE: Record<string, string> = {
  Cash: FINANCE_ACCOUNT_CODES.Cash,
  'Customer Refund Liabilities': FINANCE_ACCOUNT_CODES.CustomerRefundLiabilities,
  'Deferred Revenue': FINANCE_ACCOUNT_CODES.DeferredRevenue,
  'Hall Revenue': FINANCE_ACCOUNT_CODES.HallRevenue,
  'Non Refundable Deposit Revenue': FINANCE_ACCOUNT_CODES.NonRefundableDepositRevenue,
  'Service Revenue': FINANCE_ACCOUNT_CODES.ServiceRevenue,
  Bank: 'Bank',
};

/** English journal line descriptions persisted at posting time → localization key. */
const JOURNAL_LINE_DESCRIPTION_KEYS: Record<string, string> = {
  'Cash receipt': 'Journal:Line:CashReceipt',
  'Non-refundable deposit revenue': 'Journal:Line:DepositRevenue',
  'Deferred revenue': 'Journal:Line:DeferredRevenue',
  'Payment revenue': 'Journal:Line:PaymentRevenue',
  'Release deferred revenue': 'Journal:Line:ReleaseDeferredRevenue',
  'Hall revenue': 'Journal:Line:HallRevenue',
  'Service revenue': 'Journal:Line:ServiceRevenue',
  'Release deferred revenue for refund': 'Journal:Line:ReleaseDeferredForRefund',
  'Customer refund liability': 'Journal:Line:CustomerRefundLiability',
  'Settle customer refund liability': 'Journal:Line:SettleRefundLiability',
  'Cash refund to customer': 'Journal:Line:CashRefund',
};

/** English journal entry header descriptions → localization key + reservation capture group. */
const JOURNAL_ENTRY_DESCRIPTION_PATTERNS: Array<{
  pattern: RegExp;
  key: string;
}> = [
  {
    pattern: /^Deposit receipt for reservation (.+)$/i,
    key: 'Journal:DepositReceived',
  },
  {
    pattern: /^Full payment receipt for reservation (.+)$/i,
    key: 'Journal:FullDepositReceived',
  },
  {
    pattern: /^Installment receipt for reservation (.+)$/i,
    key: 'Journal:InstallmentReceived',
  },
  {
    pattern: /^Payment receipt for reservation (.+)$/i,
    key: 'Journal:PaymentReceived',
  },
  {
    pattern: /^Deferred revenue recognized for reservation (.+)$/i,
    key: 'Journal:RevenueRecognized',
  },
  {
    pattern: /^Refund liability for reservation (.+)$/i,
    key: 'Journal:RefundLiability',
  },
  {
    pattern: /^Refund payment for reservation (.+)$/i,
    key: 'Journal:RefundPayment',
  },
];

@Injectable({ providedIn: 'root' })
export class FinanceLocalizationService {
  private readonly l10n = inject(AppLocalizationService);

  journalSourceType(sourceType: string): string {
    if (!sourceType) {
      return '—';
    }

    const key = `Enum:JournalEntrySourceType:${sourceType}`;
    return this.translate(key, sourceType);
  }

  accountType(type: string): string {
    if (!type) {
      return '—';
    }

    const key = `Enum:AccountType:${type}`;
    return this.translate(key, type);
  }

  accountName(accountCode: string, storedName: string): string {
    const code = accountCode?.trim() || ACCOUNT_NAME_TO_CODE[storedName?.trim() ?? ''];

    if (code) {
      const byCode = this.translate(`FinanceAccount:${code}`, '');
      if (byCode) {
        return byCode;
      }
    }

    return storedName?.trim() || '—';
  }

  journalLineDescription(description: string | null | undefined): string {
    const text = description?.trim();
    if (!text) {
      return '—';
    }

    const key = JOURNAL_LINE_DESCRIPTION_KEYS[text];
    if (key) {
      return this.translate(key, text);
    }

    return text;
  }

  journalEntryDescription(description: string | null | undefined): string {
    const text = description?.trim();
    if (!text) {
      return '—';
    }

    for (const { pattern, key } of JOURNAL_ENTRY_DESCRIPTION_PATTERNS) {
      const match = text.match(pattern);
      if (match?.[1]) {
        return this.l10n.instant(key, match[1].trim());
      }
    }

    const lineKey = JOURNAL_LINE_DESCRIPTION_KEYS[text];
    if (lineKey) {
      return this.translate(lineKey, text);
    }

    return text;
  }

  private translate(key: string, fallback: string): string {
    const translated = this.l10n.instant(key);

    if (this.isMissingTranslation(translated, key)) {
      return fallback;
    }

    return translated;
  }

  private isMissingTranslation(translated: string, key: string): boolean {
    if (!translated) {
      return true;
    }

    if (translated === key || translated === `::${key}`) {
      return true;
    }

    return translated.includes('::') && translated.endsWith(key);
  }
}
