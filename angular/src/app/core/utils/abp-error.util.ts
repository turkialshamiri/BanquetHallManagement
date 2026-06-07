interface AbpErrorResponse {
  error?: {
    error?: {
      message?: string;
    };
    message?: string;
  };
}

export function getAbpErrorMessage(
  error: unknown,
  fallback = 'حدث خطأ أثناء تنفيذ العملية'
): string {
  const response = error as AbpErrorResponse;

  return (
    response?.error?.error?.message ??
    response?.error?.message ??
    fallback
  );
}
