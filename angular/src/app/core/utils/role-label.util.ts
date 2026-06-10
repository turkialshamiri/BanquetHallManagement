import { StatusLocalizationService } from '../services/status-localization.service';

export function formatRoleLabels(
  roles: string[] | null | undefined,
  statusL10n: StatusLocalizationService
): string {
  if (!roles?.length) {
    return '—';
  }

  return roles.map((role) => formatSingleRoleLabel(role, statusL10n)).join(' / ');
}

export function formatSingleRoleLabel(
  role: string,
  statusL10n: StatusLocalizationService
): string {
  return statusL10n.roleLabel(role);
}

export function normalizeEmployeeRoles(raw: unknown): string[] {
  if (Array.isArray(raw)) {
    return raw.filter((role): role is string => typeof role === 'string');
  }

  return [];
}
