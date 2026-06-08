export function formatRoleLabels(roles: string[] | null | undefined): string {
  if (!roles?.length) {
    return '—';
  }

  return roles.map(formatSingleRoleLabel).join(' / ');
}

export function formatSingleRoleLabel(role: string): string {
  const normalized = role.trim().toLowerCase();

  if (normalized === 'admin') {
    return 'مدير النظام';
  }

  if (normalized === 'employee') {
    return 'موظف';
  }

  return role;
}

export function normalizeEmployeeRoles(raw: unknown): string[] {
  if (Array.isArray(raw)) {
    return raw.filter((role): role is string => typeof role === 'string');
  }

  return [];
}
