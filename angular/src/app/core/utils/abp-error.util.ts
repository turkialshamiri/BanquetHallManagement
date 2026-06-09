interface AbpErrorPayload {
  code?: string;
  message?: string;
  details?: string;
}

interface AbpHttpErrorBody {
  error?: AbpErrorPayload;
  message?: string;
}

/**
 * Extracts a user-friendly message from an ABP / Angular HttpClient error.
 * Never returns raw exception objects or stack traces.
 */
export function getAbpErrorMessage(
  error: unknown,
  fallback = 'حدث خطأ أثناء تنفيذ العملية'
): string {
  if (!error || typeof error !== 'object') {
    return fallback;
  }

  const httpError = error as { error?: unknown; message?: string };
  const body = httpError.error;

  if (body && typeof body === 'object') {
    const abpBody = body as AbpHttpErrorBody;

    if (abpBody.error?.message) {
      return abpBody.error.message;
    }

    if (abpBody.message) {
      return abpBody.message;
    }

    const nested = (body as { error?: { error?: AbpErrorPayload } }).error?.error;
    if (nested?.message) {
      return nested.message;
    }
  }

  if (typeof httpError.message === 'string' && !httpError.message.startsWith('Http failure')) {
    return httpError.message;
  }

  return fallback;
}
